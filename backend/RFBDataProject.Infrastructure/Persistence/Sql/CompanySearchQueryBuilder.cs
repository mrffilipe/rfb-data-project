using RFBDataProject.Application.Services.Companies;
using Npgsql;

namespace RFBDataProject.Infrastructure.Persistence.Sql;

public sealed class CompanySearchQuery
{
    public required string SelectSql { get; init; }
    public required IReadOnlyList<NpgsqlParameter> Parameters { get; init; }
    public required int FetchSize { get; init; }
}

public static class CompanySearchQueryBuilder
{
    private const string LookupJoins = """
        LEFT JOIN receita_municipio_staging m
            ON m.codigo = base.municipio AND m.execution_id = base.execution_id
        LEFT JOIN receita_cnae_staging cn
            ON cn.codigo = base.cnae_fiscal_principal AND cn.execution_id = base.execution_id
        """;

    private const string SelectColumns = """
        SELECT base.cnpj_basico || base.cnpj_ordem || base.cnpj_dv AS cnpj,
               base.razao_social AS razao_social,
               base.nome_fantasia AS nome_fantasia,
               base.uf AS uf,
               m.descricao AS municipio_desc,
               base.cnae_fiscal_principal AS cnae_fiscal_principal,
               cn.descricao AS cnae_principal_desc,
               base.situacao_cadastral AS situacao_cadastral
        """;

    public static CompanySearchQuery Build(CompanyFilterRequest request, Guid executionId)
    {
        var fetchSize = request.PageSize + 1;
        var offset = (request.Page - 1) * request.PageSize;

        if (!string.IsNullOrWhiteSpace(request.Query))
            return BuildTextSearchQuery(request, executionId, fetchSize, offset);

        return BuildEstabFirstQuery(request, executionId, fetchSize, offset);
    }

    /// <summary>
    /// Paginate on estabelecimento first (index-friendly), then join empresa for ~pageSize rows.
    /// </summary>
    private static CompanySearchQuery BuildEstabFirstQuery(
        CompanyFilterRequest request,
        Guid executionId,
        int fetchSize,
        int offset)
    {
        var estabFilters = new List<string> { "e.execution_id = @execution_id" };
        var parameters = new List<NpgsqlParameter> { new("execution_id", executionId) };

        AppendEstabFilters(estabFilters, parameters, request);
        AppendEmpresaSemiJoinFilter(estabFilters, parameters, request);

        var where = string.Join(" AND ", estabFilters);
        var selectSql = $"""
            WITH page_estab AS (
                SELECT e.cnpj_basico,
                       e.cnpj_ordem,
                       e.cnpj_dv,
                       e.nome_fantasia,
                       e.uf,
                       e.municipio,
                       e.cnae_fiscal_principal,
                       e.situacao_cadastral,
                       e.execution_id
                FROM receita_estabelecimento_staging e
                WHERE {where}
                ORDER BY e.cnpj_basico, e.cnpj_ordem
                LIMIT @limit OFFSET @offset
            )
            {SelectColumns}
            FROM (
                SELECT pe.cnpj_basico,
                       pe.cnpj_ordem,
                       pe.cnpj_dv,
                       pe.nome_fantasia,
                       pe.uf,
                       pe.municipio,
                       pe.cnae_fiscal_principal,
                       pe.situacao_cadastral,
                       pe.execution_id,
                       c.razao_social
                FROM page_estab pe
                INNER JOIN receita_empresa_staging c
                    ON c.cnpj_basico = pe.cnpj_basico AND c.execution_id = pe.execution_id
            ) base
            {LookupJoins}
            ORDER BY base.cnpj_basico, base.cnpj_ordem
            """;

        return new CompanySearchQuery
        {
            SelectSql = selectSql,
            Parameters = parameters,
            FetchSize = fetchSize
        };
    }

    /// <summary>
    /// Text search: drive from empresa trigram index when possible; narrow by UF on estabelecimento.
    /// </summary>
    private static CompanySearchQuery BuildTextSearchQuery(
        CompanyFilterRequest request,
        Guid executionId,
        int fetchSize,
        int offset)
    {
        var parameters = new List<NpgsqlParameter>
        {
            new("execution_id", executionId),
            new("q", $"%{request.Query!.Trim()}%")
        };

        var estabFilters = new List<string> { "e.execution_id = @execution_id" };
        AppendEstabFilters(estabFilters, parameters, request);
        AppendEmpresaSemiJoinFilter(estabFilters, parameters, request);

        var estabWhere = string.Join(" AND ", estabFilters);
        var hasUf = !string.IsNullOrWhiteSpace(request.StateCode);

        string selectSql;
        if (hasUf)
        {
            selectSql = $"""
                WITH empresa_match AS (
                    SELECT cnpj_basico, razao_social
                    FROM receita_empresa_staging
                    WHERE execution_id = @execution_id AND razao_social ILIKE @q
                ),
                from_razao AS (
                    SELECT e.cnpj_basico, e.cnpj_ordem, e.cnpj_dv, e.nome_fantasia, e.uf,
                           e.municipio, e.cnae_fiscal_principal, e.situacao_cadastral,
                           e.execution_id, em.razao_social
                    FROM receita_estabelecimento_staging e
                    INNER JOIN empresa_match em ON em.cnpj_basico = e.cnpj_basico
                    WHERE {estabWhere}
                ),
                from_fantasia AS (
                    SELECT e.cnpj_basico, e.cnpj_ordem, e.cnpj_dv, e.nome_fantasia, e.uf,
                           e.municipio, e.cnae_fiscal_principal, e.situacao_cadastral,
                           e.execution_id, c.razao_social
                    FROM receita_estabelecimento_staging e
                    INNER JOIN receita_empresa_staging c
                        ON c.cnpj_basico = e.cnpj_basico AND c.execution_id = e.execution_id
                    WHERE {estabWhere} AND e.nome_fantasia ILIKE @q
                ),
                combined AS (
                    SELECT * FROM from_razao
                    UNION
                    SELECT * FROM from_fantasia
                ),
                page AS (
                    SELECT * FROM combined
                    ORDER BY cnpj_basico, cnpj_ordem
                    LIMIT @limit OFFSET @offset
                )
                {SelectColumns}
                FROM page base
                {LookupJoins}
                ORDER BY base.cnpj_basico, base.cnpj_ordem
                """;
        }
        else
        {
            selectSql = $"""
                WITH empresa_match AS (
                    SELECT cnpj_basico, razao_social
                    FROM receita_empresa_staging
                    WHERE execution_id = @execution_id AND razao_social ILIKE @q
                ),
                from_razao AS (
                    SELECT e.cnpj_basico, e.cnpj_ordem, e.cnpj_dv, e.nome_fantasia, e.uf,
                           e.municipio, e.cnae_fiscal_principal, e.situacao_cadastral,
                           e.execution_id, em.razao_social
                    FROM receita_estabelecimento_staging e
                    INNER JOIN empresa_match em ON em.cnpj_basico = e.cnpj_basico
                    WHERE e.execution_id = @execution_id
                ),
                from_fantasia AS (
                    SELECT e.cnpj_basico, e.cnpj_ordem, e.cnpj_dv, e.nome_fantasia, e.uf,
                           e.municipio, e.cnae_fiscal_principal, e.situacao_cadastral,
                           e.execution_id, c.razao_social
                    FROM receita_estabelecimento_staging e
                    INNER JOIN receita_empresa_staging c
                        ON c.cnpj_basico = e.cnpj_basico AND c.execution_id = e.execution_id
                    WHERE e.execution_id = @execution_id AND e.nome_fantasia ILIKE @q
                ),
                combined AS (
                    SELECT * FROM from_razao
                    UNION
                    SELECT * FROM from_fantasia
                ),
                page AS (
                    SELECT * FROM combined
                    ORDER BY cnpj_basico, cnpj_ordem
                    LIMIT @limit OFFSET @offset
                )
                {SelectColumns}
                FROM page base
                {LookupJoins}
                ORDER BY base.cnpj_basico, base.cnpj_ordem
                """;
        }

        return new CompanySearchQuery
        {
            SelectSql = selectSql,
            Parameters = parameters,
            FetchSize = fetchSize
        };
    }

    private static void AppendEstabFilters(
        List<string> filters,
        List<NpgsqlParameter> parameters,
        CompanyFilterRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.StateCode))
        {
            filters.Add("e.uf = @uf");
            parameters.Add(new NpgsqlParameter("uf", request.StateCode.Trim().ToUpperInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(request.Cnae))
        {
            filters.Add("e.cnae_fiscal_principal = @cnae");
            parameters.Add(new NpgsqlParameter("cnae", request.Cnae.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(request.RegistrationStatus))
        {
            filters.Add("e.situacao_cadastral = @situacao");
            parameters.Add(new NpgsqlParameter("situacao", request.RegistrationStatus.Trim()));
        }

        if (request.HeadOfficeOnly)
            filters.Add("e.identificador_matriz_filial = '1'");
    }

    private static void AppendEmpresaSemiJoinFilter(
        List<string> estabFilters,
        List<NpgsqlParameter> parameters,
        CompanyFilterRequest request)
    {
        var empresaConditions = new List<string> { "c.execution_id = @execution_id" };

        if (!string.IsNullOrWhiteSpace(request.LegalNatureCode))
        {
            empresaConditions.Add("c.natureza_juridica = @natureza");
            parameters.Add(new NpgsqlParameter("natureza", request.LegalNatureCode.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(request.CompanySizeCode))
        {
            empresaConditions.Add("c.porte_empresa = @porte");
            parameters.Add(new NpgsqlParameter("porte", request.CompanySizeCode.Trim()));
        }

        if (request.ShareCapitalMin.HasValue)
        {
            empresaConditions.Add("NULLIF(REPLACE(c.capital_social, ',', '.'), '')::numeric >= @capitalMin");
            parameters.Add(new NpgsqlParameter("capitalMin", request.ShareCapitalMin.Value));
        }

        if (request.ShareCapitalMax.HasValue)
        {
            empresaConditions.Add("NULLIF(REPLACE(c.capital_social, ',', '.'), '')::numeric <= @capitalMax");
            parameters.Add(new NpgsqlParameter("capitalMax", request.ShareCapitalMax.Value));
        }

        if (empresaConditions.Count == 1)
            return;

        var innerWhere = string.Join(" AND ", empresaConditions);
        estabFilters.Add(
            $"e.cnpj_basico IN (SELECT c.cnpj_basico FROM receita_empresa_staging c WHERE {innerWhere})");
    }
}
