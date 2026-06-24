using RFBDataProject.Application.Common;
using RFBDataProject.Application.Exceptions;
using RFBDataProject.Application.Services.Companies;
using RFBDataProject.Domain.Rules;
using RFBDataProject.Infrastructure.Persistence;
using RFBDataProject.Infrastructure.Persistence.Sql;
using RFBDataProject.Infrastructure.Services.Staging;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Services.Companies;

public sealed class CompanyQueryService : ICompanyQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly IStagingExecutionResolver _executionResolver;

    public CompanyQueryService(ApplicationDbContext context, IStagingExecutionResolver executionResolver)
    {
        _context = context;
        _executionResolver = executionResolver;
    }

    public async Task<CompanyDetailDto?> GetByCnpjAsync(GetCompanyByCnpjRequest request, CancellationToken ct = default)
    {
        var cnpj = CnpjValidationRules.NormalizeDigits(request.Cnpj);
        CnpjValidationRules.ValidateCnpj(cnpj);
        var executionId = await _executionResolver.GetActiveExecutionIdAsync(ct);

        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            $"""
             {StagingQuerySql.CompanyCompleteSelect}
             {StagingQuerySql.CompanyCompleteFrom}
             WHERE c.execution_id = @execution_id
               AND c.cnpj_basico || e.cnpj_ordem || e.cnpj_dv = @cnpj
             LIMIT 1
             """, conn);
        cmd.Parameters.AddWithValue("execution_id", executionId);
        cmd.Parameters.AddWithValue("cnpj", cnpj);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        string? S(int i) => reader.IsDBNull(i) ? null : reader.GetString(i);

        return new CompanyDetailDto
        {
            Cnpj = reader.GetString(0),
            LegalName = reader.GetString(1),
            TradeName = S(2),
            LegalNatureCode = S(3),
            LegalNatureDescription = S(4),
            ShareCapital = S(5),
            CompanySizeCode = S(6),
            RegistrationStatus = S(7),
            ActivityStartDate = S(8),
            PrimaryCnaeCode = S(9),
            PrimaryCnaeDescription = S(10),
            StateCode = S(12),
            MunicipalityCode = S(13),
            MunicipalityDescription = S(14),
            StreetName = S(15),
            StreetNumber = S(16),
            Neighborhood = S(17),
            ZipCode = S(18),
            Email = S(19),
            PhoneNumber = S(20)
        };
    }

    public async Task<PagedResult<CompanySummaryDto>> ListHoldingsAsync(ListHoldingsRequest request, CancellationToken ct = default)
    {
        if (request.Page < 1)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.INVALID_PAGE);
        if (request.PageSize is < 1 or > 100)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.INVALID_PAGE_SIZE);

        var executionId = await _executionResolver.GetActiveExecutionIdAsync(ct);
        var offset = (request.Page - 1) * request.PageSize;
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        var filters = new List<string>();
        var parameters = new List<NpgsqlParameter> { new("execution_id", executionId) };
        if (!string.IsNullOrWhiteSpace(request.StateCode))
        {
            filters.Add("h.uf = @uf");
            parameters.Add(new NpgsqlParameter("uf", request.StateCode.Trim().ToUpperInvariant()));
        }

        var where = filters.Count > 0 ? "WHERE " + string.Join(" AND ", filters) : string.Empty;
        var countSql = $"""
            SELECT COUNT(*) FROM (
                {StagingQuerySql.HoldingsSelect}
                {StagingQuerySql.HoldingsFrom}
            ) h {where}
            """;
        var sql = $"""
            SELECT h.cnpj, h.razao_social, NULL, h.uf, m.descricao, h.cnae_fiscal_principal, NULL, NULL
            FROM (
                {StagingQuerySql.HoldingsSelect}
                {StagingQuerySql.HoldingsFrom}
            ) h
            LEFT JOIN receita_municipio_staging m
                ON m.codigo = h.municipio AND m.execution_id = @execution_id
            {where}
            ORDER BY h.cnpj
            LIMIT @limit OFFSET @offset
            """;

        parameters.Add(new NpgsqlParameter("limit", request.PageSize));
        parameters.Add(new NpgsqlParameter("offset", offset));

        await using var countCmd = new NpgsqlCommand(countSql, conn);
        NpgsqlCommandBinder.AddParameters(countCmd, parameters, excludePaging: true);
        var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct) ?? 0);

        await using var cmd = new NpgsqlCommand(sql, conn);
        NpgsqlCommandBinder.AddParameters(cmd, parameters);

        var items = new List<CompanySummaryDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new CompanySummaryDto
            {
                Cnpj = reader.GetString(0),
                LegalName = reader.GetString(1),
                TradeName = null,
                StateCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                Municipality = reader.IsDBNull(4) ? null : reader.GetString(4),
                PrimaryCnaeCode = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return new PagedResult<CompanySummaryDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }
}
