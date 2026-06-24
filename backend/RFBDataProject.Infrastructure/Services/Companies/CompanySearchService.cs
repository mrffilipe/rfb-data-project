using RFBDataProject.Application.Common;
using RFBDataProject.Application.Exceptions;
using RFBDataProject.Application.Services.Companies;
using RFBDataProject.Infrastructure.Caching;
using RFBDataProject.Infrastructure.Persistence;
using RFBDataProject.Infrastructure.Persistence.Sql;
using RFBDataProject.Infrastructure.Services.Staging;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Services.Companies;

public sealed class CompanySearchService : ICompanySearchService
{
    private readonly ApplicationDbContext _context;
    private readonly SearchCacheService _cache;
    private readonly IStagingExecutionResolver _executionResolver;

    public CompanySearchService(
        ApplicationDbContext context,
        SearchCacheService cache,
        IStagingExecutionResolver executionResolver)
    {
        _context = context;
        _cache = cache;
        _executionResolver = executionResolver;
    }

    public Task<PagedResult<CompanySummaryDto>> SearchAsync(SearchCompaniesRequest request, CancellationToken ct = default)
    {
        ValidateQuery(request);
        return QueryAsync(request, ct);
    }

    public Task<PagedResult<CompanySummaryDto>> ListAsync(ListCompaniesRequest request, CancellationToken ct = default)
    {
        ValidateQuery(request);
        return QueryAsync(request, ct);
    }

    private async Task<PagedResult<CompanySummaryDto>> QueryAsync(
        CompanyFilterRequest request,
        CancellationToken ct)
    {
        ValidatePaged(request);

        var cached = await _cache.GetAsync<PagedResult<CompanySummaryDto>>("company-search:", request, ct);
        if (cached is not null)
            return cached;

        var executionId = await _executionResolver.GetActiveExecutionIdAsync(ct);
        var query = CompanySearchQueryBuilder.Build(request, executionId);

        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        var parameters = query.Parameters.ToList();
        parameters.Add(new NpgsqlParameter("limit", query.FetchSize));
        parameters.Add(new NpgsqlParameter("offset", (request.Page - 1) * request.PageSize));

        var items = await ExecuteSummaryQueryAsync(conn, query.SelectSql, parameters, ct);
        var hasMore = items.Count > request.PageSize;
        if (hasMore)
            items = items.Take(request.PageSize).ToList();

        var (total, isApproximate) = hasMore
            ? (SearchConstants.ApproximateDisplayCap, true)
            : ((request.Page - 1) * request.PageSize + items.Count, false);

        var result = new PagedResult<CompanySummaryDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total,
            TotalIsApproximate = isApproximate,
            Items = items
        };

        await _cache.SetAsync("company-search:", request, result, ct);
        return result;
    }

    private static void ValidateQuery(CompanyFilterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return;

        var q = request.Query.Trim();
        if (q.Length < 2)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.QUERY_TOO_SHORT);
        if (q.Length > 200)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.QUERY_TOO_LONG);
    }

    private static async Task<List<CompanySummaryDto>> ExecuteSummaryQueryAsync(
        NpgsqlConnection conn,
        string sql,
        List<NpgsqlParameter> parameters,
        CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(sql, conn) { CommandTimeout = 120 };
        NpgsqlCommandBinder.AddParameters(cmd, parameters);

        var items = new List<CompanySummaryDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new CompanySummaryDto
            {
                Cnpj = reader.GetString(0),
                LegalName = reader.GetString(1),
                TradeName = reader.IsDBNull(2) ? null : reader.GetString(2),
                StateCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                Municipality = reader.IsDBNull(4) ? null : reader.GetString(4),
                PrimaryCnaeCode = reader.IsDBNull(5) ? null : reader.GetString(5),
                PrimaryCnaeDescription = reader.IsDBNull(6) ? null : reader.GetString(6),
                RegistrationStatus = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }

        return items;
    }

    private static void ValidatePaged(PagedRequest request)
    {
        if (request.Page < 1)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.INVALID_PAGE);
        if (request.PageSize is < 1 or > 100)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.INVALID_PAGE_SIZE);
    }
}
