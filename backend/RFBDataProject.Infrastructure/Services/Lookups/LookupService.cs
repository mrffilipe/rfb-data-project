using RFBDataProject.Application.Common;
using RFBDataProject.Application.Exceptions;
using RFBDataProject.Application.Services.Lookups;
using RFBDataProject.Domain.Constants;
using RFBDataProject.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Services.Lookups;

public sealed class LookupService : ILookupService
{
    private readonly ApplicationDbContext _context;

    public LookupService(ApplicationDbContext context) => _context = context;

    public IReadOnlyList<LookupItemDto> ListStates() =>
        MapCatalog(LookupCatalogs.BrazilianStates);

    public IReadOnlyList<LookupItemDto> ListRegistrationStatuses() =>
        MapCatalog(LookupCatalogs.RegistrationStatuses);

    public IReadOnlyList<LookupItemDto> ListCompanySizes() =>
        MapCatalog(LookupCatalogs.CompanySizes);

    public Task<PagedResult<LookupItemDto>> SearchCnaesAsync(LookupSearchRequest request, CancellationToken ct = default) =>
        SearchTableAsync("cnaes", request, ct);

    public Task<PagedResult<LookupItemDto>> SearchMunicipalitiesAsync(LookupSearchRequest request, CancellationToken ct = default) =>
        SearchTableAsync("municipios", request, ct);

    public Task<PagedResult<LookupItemDto>> SearchLegalNaturesAsync(LookupSearchRequest request, CancellationToken ct = default) =>
        SearchTableAsync("naturezas", request, ct);

    public Task<PagedResult<LookupItemDto>> SearchQualificationsAsync(LookupSearchRequest request, CancellationToken ct = default) =>
        SearchTableAsync("qualificacoes", request, ct);

    private async Task<PagedResult<LookupItemDto>> SearchTableAsync(
        string table,
        LookupSearchRequest request,
        CancellationToken ct)
    {
        if (request.Page < 1)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.INVALID_PAGE);
        if (request.PageSize is < 1 or > 100)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.INVALID_PAGE_SIZE);
        if (string.IsNullOrWhiteSpace(request.Query))
            throw new ApplicationValidationException(ApplicationErrorMessages.Lookup.QUERY_TOO_SHORT);

        var q = request.Query.Trim();
        var pattern = $"%{q}%";
        var offset = (request.Page - 1) * request.PageSize;

        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        var countSql = $"""
            SELECT COUNT(*) FROM {table}
            WHERE codigo ILIKE @q OR descricao ILIKE @q
            """;

        var sql = $"""
            SELECT codigo, descricao FROM {table}
            WHERE codigo ILIKE @q OR descricao ILIKE @q
            ORDER BY descricao
            LIMIT @limit OFFSET @offset
            """;

        await using var countCmd = new NpgsqlCommand(countSql, conn);
        countCmd.Parameters.AddWithValue("q", pattern);
        var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct) ?? 0);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("q", pattern);
        cmd.Parameters.AddWithValue("limit", request.PageSize);
        cmd.Parameters.AddWithValue("offset", offset);

        var items = new List<LookupItemDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new LookupItemDto
            {
                Code = reader.GetString(0),
                Description = reader.GetString(1)
            });
        }

        return new PagedResult<LookupItemDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

    private static IReadOnlyList<LookupItemDto> MapCatalog(IReadOnlyList<LookupCatalogs.LookupEntry> entries) =>
        entries.Select(e => new LookupItemDto { Code = e.Code, Description = e.Description }).ToList();
}
