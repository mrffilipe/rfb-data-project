using RFBDataProject.Application.Common;
using RFBDataProject.Application.Exceptions;
using RFBDataProject.Application.Services.Companies;
using RFBDataProject.Infrastructure.Caching;
using RFBDataProject.Infrastructure.Persistence;
using RFBDataProject.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Services.Companies;

public sealed class CompanySearchService : ICompanySearchService
{
    private readonly ApplicationDbContext _context;
    private readonly SearchCacheService _cache;

    public CompanySearchService(ApplicationDbContext context, SearchCacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<PagedResult<CompanySummaryDto>> SearchAsync(SearchCompaniesRequest request, CancellationToken ct = default)
    {
        ValidatePaged(request);
        if (string.IsNullOrWhiteSpace(request.Query))
            return await ListInternalAsync(BuildListRequest(request), ct);

        var q = request.Query.Trim();
        if (q.Length < 2)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.QUERY_TOO_SHORT);
        if (q.Length > 200)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.QUERY_TOO_LONG);

        var cached = await _cache.GetAsync<PagedResult<CompanySummaryDto>>("search:", request, ct);
        if (cached is not null)
            return cached;

        var offset = (request.Page - 1) * request.PageSize;
        var pattern = $"%{q}%";

        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        var filters = new List<string> { "(v.razao_social ILIKE @q OR v.nome_fantasia ILIKE @q)" };
        var parameters = new List<NpgsqlParameter> { new("q", pattern) };
        AddCommonFilters(filters, parameters, request);

        var where = "WHERE " + string.Join(" AND ", filters);
        var countSql = $"SELECT COUNT(*) FROM vw_empresas_completo v {where}";
        var sql = $"""
            SELECT v.cnpj, v.razao_social, v.nome_fantasia, v.uf, v.municipio_desc,
                   v.cnae_fiscal_principal, v.cnae_principal_desc, v.situacao_cadastral
            FROM vw_empresas_completo v
            {where}
            ORDER BY v.cnpj
            LIMIT @limit OFFSET @offset
            """;

        parameters.Add(new NpgsqlParameter("limit", request.PageSize));
        parameters.Add(new NpgsqlParameter("offset", offset));

        var total = await ExecuteCountAsync(conn, countSql, parameters, ct);
        var items = await ExecuteSummaryQueryAsync(conn, sql, parameters, ct);
        var result = BuildPagedResult(items, request, total);

        await _cache.SetAsync("search:", request, result, ct);
        return result;
    }

    public Task<PagedResult<CompanySummaryDto>> ListAsync(ListCompaniesRequest request, CancellationToken ct = default)
    {
        ValidatePaged(request);
        return ListInternalAsync(request, ct);
    }

    private async Task<PagedResult<CompanySummaryDto>> ListInternalAsync(
        ListCompaniesRequest request,
        CancellationToken ct)
    {
        var offset = (request.Page - 1) * request.PageSize;
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        var filters = new List<string>();
        var parameters = new List<NpgsqlParameter>();
        AddListFilters(filters, parameters, request);

        var where = filters.Count > 0 ? "WHERE " + string.Join(" AND ", filters) : string.Empty;
        var countSql = $"""
            SELECT COUNT(*)
            FROM estabelecimentos est
            JOIN empresas e ON e.cnpj_basico = est.cnpj_basico
            LEFT JOIN municipios m ON m.codigo = est.municipio
            {where}
            """;

        var sql = $"""
            SELECT est.cnpj_basico || est.cnpj_ordem || est.cnpj_dv,
                   e.razao_social, est.nome_fantasia, est.uf, m.descricao,
                   est.cnae_fiscal_principal, NULL, est.situacao_cadastral
            FROM estabelecimentos est
            JOIN empresas e ON e.cnpj_basico = est.cnpj_basico
            LEFT JOIN municipios m ON m.codigo = est.municipio
            {where}
            ORDER BY est.cnpj_basico, est.cnpj_ordem
            LIMIT @limit OFFSET @offset
            """;

        parameters.Add(new NpgsqlParameter("limit", request.PageSize));
        parameters.Add(new NpgsqlParameter("offset", offset));

        var total = await ExecuteCountAsync(conn, countSql, parameters, ct);
        var items = await ExecuteSummaryQueryAsync(conn, sql, parameters, ct);
        return BuildPagedResult(items, request, total);
    }

    private static ListCompaniesRequest BuildListRequest(SearchCompaniesRequest request) => new()
    {
        Page = request.Page,
        PageSize = request.PageSize,
        StateCode = request.StateCode,
        Cnae = request.Cnae,
        LegalNatureCode = request.LegalNatureCode,
        HeadOfficeOnly = request.HeadOfficeOnly
    };

    private static void AddCommonFilters(List<string> filters, List<NpgsqlParameter> parameters, SearchCompaniesRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.StateCode))
        {
            filters.Add("v.uf = @uf");
            parameters.Add(new NpgsqlParameter("uf", request.StateCode.Trim().ToUpperInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(request.Cnae))
        {
            filters.Add("v.cnae_fiscal_principal = @cnae");
            parameters.Add(new NpgsqlParameter("cnae", request.Cnae.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(request.LegalNatureCode))
        {
            filters.Add("v.natureza_juridica = @natureza");
            parameters.Add(new NpgsqlParameter("natureza", request.LegalNatureCode.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(request.RegistrationStatus))
        {
            filters.Add("v.situacao_cadastral = @situacao");
            parameters.Add(new NpgsqlParameter("situacao", request.RegistrationStatus.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(request.CompanySizeCode))
        {
            filters.Add("v.porte_empresa = @porte");
            parameters.Add(new NpgsqlParameter("porte", request.CompanySizeCode.Trim()));
        }

        if (request.ShareCapitalMin.HasValue)
        {
            filters.Add("NULLIF(REPLACE(v.capital_social, ',', '.'), '')::numeric >= @capitalMin");
            parameters.Add(new NpgsqlParameter("capitalMin", request.ShareCapitalMin.Value));
        }

        if (request.ShareCapitalMax.HasValue)
        {
            filters.Add("NULLIF(REPLACE(v.capital_social, ',', '.'), '')::numeric <= @capitalMax");
            parameters.Add(new NpgsqlParameter("capitalMax", request.ShareCapitalMax.Value));
        }

        if (request.HeadOfficeOnly)
            filters.Add("v.identificador_matriz_filial = '1'");
    }

    private static void AddListFilters(List<string> filters, List<NpgsqlParameter> parameters, ListCompaniesRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.StateCode))
        {
            filters.Add("est.uf = @uf");
            parameters.Add(new NpgsqlParameter("uf", request.StateCode.Trim().ToUpperInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(request.Cnae))
        {
            filters.Add("est.cnae_fiscal_principal = @cnae");
            parameters.Add(new NpgsqlParameter("cnae", request.Cnae.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(request.LegalNatureCode))
        {
            filters.Add("e.natureza_juridica = @natureza");
            parameters.Add(new NpgsqlParameter("natureza", request.LegalNatureCode.Trim()));
        }

        if (request.HeadOfficeOnly)
            filters.Add("est.identificador_matriz_filial = '1'");
    }

    private static async Task<int> ExecuteCountAsync(
        NpgsqlConnection conn,
        string sql,
        List<NpgsqlParameter> parameters,
        CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(sql, conn);
        NpgsqlCommandBinder.AddParameters(cmd, parameters, excludePaging: true);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct) ?? 0);
    }

    private static async Task<List<CompanySummaryDto>> ExecuteSummaryQueryAsync(
        NpgsqlConnection conn,
        string sql,
        List<NpgsqlParameter> parameters,
        CancellationToken ct)
    {
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

    private static PagedResult<CompanySummaryDto> BuildPagedResult(
        IReadOnlyList<CompanySummaryDto> items,
        PagedRequest request,
        int total) => new()
    {
        Page = request.Page,
        PageSize = request.PageSize,
        Total = total,
        Items = items
    };

    private static void ValidatePaged(PagedRequest request)
    {
        if (request.Page < 1)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.INVALID_PAGE);
        if (request.PageSize is < 1 or > 100)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.INVALID_PAGE_SIZE);
    }
}
