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

    private static readonly IReadOnlyDictionary<string, string> CompanySizePorteMap = new Dictionary<string, string>
    {
        ["2"] = "01",
        ["3"] = "03",
        ["4"] = "05",
        ["5"] = "05"
    };

    public static CompanySearchQuery Build(CompanyFilterRequest request, Guid executionId)
    {
        var fetchSize = request.PageSize + 1;
        var offset = (request.Page - 1) * request.PageSize;

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            return request.ExcludeQuery
                ? BuildExcludeTextQuery(request, executionId, fetchSize, offset)
                : BuildTextSearchQuery(request, executionId, fetchSize, offset);
        }

        return BuildEstabFirstQuery(request, executionId, fetchSize, offset);
    }

    public static CompanySearchQuery BuildExport(CompanyFilterRequest request, Guid executionId)
    {
        var batchSize = request.PageSize;
        var offset = (request.Page - 1) * request.PageSize;

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            return request.ExcludeQuery
                ? BuildExportExcludeTextQuery(request, executionId, batchSize, offset)
                : BuildExportTextSearchQuery(request, executionId, batchSize, offset);
        }

        return BuildExportEstabFirstQuery(request, executionId, batchSize, offset);
    }

    private const string ExportLookupJoins = """
        LEFT JOIN receita_natureza_staging ln
            ON ln.codigo = base.natureza_juridica AND ln.execution_id = base.execution_id
        LEFT JOIN receita_municipio_staging m
            ON m.codigo = base.municipio AND m.execution_id = base.execution_id
        LEFT JOIN receita_cnae_staging cn
            ON cn.codigo = base.cnae_fiscal_principal AND cn.execution_id = base.execution_id
        """;

    private const string ExportSelectColumns = """
        SELECT base.cnpj_basico || base.cnpj_ordem || base.cnpj_dv AS cnpj,
               base.nome_socio AS nome_socio,
               base.cnpj_cpf_socio AS cnpj_cpf_socio,
               base.identificador_socio AS identificador_socio,
               base.qualificacao_socio AS qualificacao_socio,
               base.data_entrada_sociedade AS data_entrada_sociedade,
               base.razao_social AS razao_social,
               base.nome_fantasia AS nome_fantasia,
               base.natureza_juridica AS natureza_juridica,
               ln.descricao AS natureza_juridica_desc,
               base.capital_social AS capital_social,
               base.porte_empresa AS porte_empresa,
               base.situacao_cadastral AS situacao_cadastral,
               base.data_inicio_atividade AS data_inicio_atividade,
               base.cnae_fiscal_principal AS cnae_fiscal_principal,
               cn.descricao AS cnae_principal_desc,
               base.identificador_matriz_filial AS identificador_matriz_filial,
               base.uf AS uf,
               base.municipio AS municipio,
               m.descricao AS municipio_desc,
               base.logradouro AS logradouro,
               base.numero AS numero,
               base.bairro AS bairro,
               base.cep AS cep,
               base.correio_eletronico AS correio_eletronico,
               CASE WHEN base.telefone_1 IS NOT NULL AND base.telefone_1 <> ''
                    THEN COALESCE(base.ddd_1, '') || base.telefone_1 ELSE NULL END AS telefone_1
        """;

    private const string ExportEstabInnerColumns = """
        e.cnpj_basico,
        e.cnpj_ordem,
        e.cnpj_dv,
        e.nome_fantasia,
        e.situacao_cadastral,
        e.data_inicio_atividade,
        e.cnae_fiscal_principal,
        e.identificador_matriz_filial,
        e.uf,
        e.municipio,
        e.logradouro,
        e.numero,
        e.bairro,
        e.cep,
        e.correio_eletronico,
        e.ddd_1,
        e.telefone_1,
        e.execution_id,
        p.nome_socio,
        p.cnpj_cpf_socio,
        p.identificador_socio,
        p.qualificacao_socio,
        p.data_entrada_sociedade,
        c.razao_social,
        c.natureza_juridica,
        c.capital_social,
        c.porte_empresa
        """;

    private static CompanySearchQuery BuildExportEstabFirstQuery(
        CompanyFilterRequest request,
        Guid executionId,
        int batchSize,
        int offset)
    {
        var estabFilters = new List<string> { "e.execution_id = @execution_id" };
        var parameters = new List<NpgsqlParameter> { new("execution_id", executionId) };

        AppendEstabFilters(estabFilters, parameters, request);
        AppendExportEmailOnlyFilter(estabFilters);
        AppendEmpresaSemiJoinFilter(estabFilters, parameters, request);

        var where = string.Join(" AND ", estabFilters);
        var selectSql = $"""
            WITH page_estab AS (
                SELECT {ExportEstabInnerColumns}
                FROM receita_estabelecimento_staging e
                INNER JOIN receita_empresa_staging c
                    ON c.cnpj_basico = e.cnpj_basico AND c.execution_id = e.execution_id
                INNER JOIN receita_socio_staging p
                    ON p.cnpj_basico = e.cnpj_basico AND p.execution_id = e.execution_id
                WHERE {where}
                ORDER BY e.cnpj_basico, e.cnpj_ordem, p.nome_socio
                LIMIT @limit OFFSET @offset
            )
            {ExportSelectColumns}
            FROM page_estab base
            {ExportLookupJoins}
            ORDER BY base.cnpj_basico, base.cnpj_ordem, base.nome_socio
            """;

        return new CompanySearchQuery
        {
            SelectSql = selectSql,
            Parameters = parameters,
            FetchSize = batchSize
        };
    }

    private static CompanySearchQuery BuildExportExcludeTextQuery(
        CompanyFilterRequest request,
        Guid executionId,
        int batchSize,
        int offset)
    {
        var parameters = new List<NpgsqlParameter>
        {
            new("execution_id", executionId),
            new("q", $"%{request.Query!.Trim()}%")
        };

        var estabFilters = new List<string>
        {
            "e.execution_id = @execution_id",
            "NOT (c.razao_social ILIKE @q OR COALESCE(e.nome_fantasia, '') ILIKE @q)"
        };

        AppendEstabFilters(estabFilters, parameters, request, skipTextJoin: true);
        AppendExportEmailOnlyFilter(estabFilters);
        AppendEmpresaSemiJoinFilter(estabFilters, parameters, request, skipLegalNatureAndCapital: true);

        if (HasValues(request.LegalNatureCodes))
        {
            AppendStringArrayFilterToList(
                estabFilters, parameters, request.LegalNatureCodes, request.ExcludeLegalNatureCodes,
                "c.natureza_juridica", "natureza", v => v.Trim());
        }

        AppendShareCapitalFilter(estabFilters, parameters, request, "c");

        var where = string.Join(" AND ", estabFilters);
        var selectSql = $"""
            WITH page_estab AS (
                SELECT {ExportEstabInnerColumns}
                FROM receita_estabelecimento_staging e
                INNER JOIN receita_empresa_staging c
                    ON c.cnpj_basico = e.cnpj_basico AND c.execution_id = e.execution_id
                INNER JOIN receita_socio_staging p
                    ON p.cnpj_basico = e.cnpj_basico AND p.execution_id = e.execution_id
                WHERE {where}
                ORDER BY e.cnpj_basico, e.cnpj_ordem, p.nome_socio
                LIMIT @limit OFFSET @offset
            )
            {ExportSelectColumns}
            FROM page_estab base
            {ExportLookupJoins}
            ORDER BY base.cnpj_basico, base.cnpj_ordem, base.nome_socio
            """;

        return new CompanySearchQuery
        {
            SelectSql = selectSql,
            Parameters = parameters,
            FetchSize = batchSize
        };
    }

    private static CompanySearchQuery BuildExportTextSearchQuery(
        CompanyFilterRequest request,
        Guid executionId,
        int batchSize,
        int offset)
    {
        var parameters = new List<NpgsqlParameter>
        {
            new("execution_id", executionId),
            new("q", $"%{request.Query!.Trim()}%")
        };

        var estabFilters = new List<string> { "e.execution_id = @execution_id" };
        AppendEstabFilters(estabFilters, parameters, request);
        AppendExportEmailOnlyFilter(estabFilters);
        AppendEmpresaSemiJoinFilter(estabFilters, parameters, request);

        var estabWhere = string.Join(" AND ", estabFilters);
        var exportInner = $"""
            SELECT {ExportEstabInnerColumns}
            FROM receita_estabelecimento_staging e
            INNER JOIN receita_empresa_staging c
                ON c.cnpj_basico = e.cnpj_basico AND c.execution_id = e.execution_id
            INNER JOIN receita_socio_staging p
                ON p.cnpj_basico = e.cnpj_basico AND p.execution_id = e.execution_id
            """;

        var selectSql = $"""
            WITH empresa_match AS (
                SELECT cnpj_basico, razao_social
                FROM receita_empresa_staging
                WHERE execution_id = @execution_id AND razao_social ILIKE @q
            ),
            from_razao AS (
                {exportInner}
                INNER JOIN empresa_match em ON em.cnpj_basico = e.cnpj_basico
                WHERE {estabWhere}
            ),
            from_fantasia AS (
                {exportInner}
                WHERE {estabWhere} AND e.nome_fantasia ILIKE @q
            ),
            combined AS (
                SELECT * FROM from_razao
                UNION
                SELECT * FROM from_fantasia
            ),
            page AS (
                SELECT * FROM combined
                ORDER BY cnpj_basico, cnpj_ordem, nome_socio
                LIMIT @limit OFFSET @offset
            )
            {ExportSelectColumns}
            FROM page base
            {ExportLookupJoins}
            ORDER BY base.cnpj_basico, base.cnpj_ordem, base.nome_socio
            """;

        return new CompanySearchQuery
        {
            SelectSql = selectSql,
            Parameters = parameters,
            FetchSize = batchSize
        };
    }

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

    private static CompanySearchQuery BuildExcludeTextQuery(
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

        var estabFilters = new List<string>
        {
            "e.execution_id = @execution_id",
            "NOT (c.razao_social ILIKE @q OR COALESCE(e.nome_fantasia, '') ILIKE @q)"
        };

        AppendEstabFilters(estabFilters, parameters, request, skipTextJoin: true);
        AppendEmpresaSemiJoinFilter(estabFilters, parameters, request, skipLegalNatureAndCapital: true);

        if (HasValues(request.LegalNatureCodes))
        {
            AppendStringArrayFilterToList(
                estabFilters, parameters, request.LegalNatureCodes, request.ExcludeLegalNatureCodes,
                "c.natureza_juridica", "natureza", v => v.Trim());
        }

        if (AppendShareCapitalFilter(estabFilters, parameters, request, "c"))
        {
            // applied directly on joined empresa alias
        }

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
                       e.execution_id,
                       c.razao_social
                FROM receita_estabelecimento_staging e
                INNER JOIN receita_empresa_staging c
                    ON c.cnpj_basico = e.cnpj_basico AND c.execution_id = e.execution_id
                WHERE {where}
                ORDER BY e.cnpj_basico, e.cnpj_ordem
                LIMIT @limit OFFSET @offset
            )
            {SelectColumns}
            FROM page_estab base
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
        var hasStates = HasValues(request.StateCodes);

        string selectSql;
        if (hasStates)
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

    private static void AppendExportEmailOnlyFilter(List<string> filters)
    {
        filters.Add("e.correio_eletronico IS NOT NULL AND BTRIM(e.correio_eletronico) <> ''");
    }

    private static void AppendEstabFilters(
        List<string> filters,
        List<NpgsqlParameter> parameters,
        CompanyFilterRequest request,
        bool skipTextJoin = false)
    {
        AppendStringArrayFilter(
            filters, parameters, request.StateCodes, request.ExcludeStates,
            "e.uf", "uf", v => v.Trim().ToUpperInvariant());

        AppendStringArrayFilter(
            filters, parameters, request.Cnaes, request.ExcludeCnaes,
            "e.cnae_fiscal_principal", "cnae", v => v.Trim());

        AppendStringArrayFilter(
            filters, parameters, request.RegistrationStatuses, request.ExcludeRegistrationStatuses,
            "e.situacao_cadastral", "situacao", v => v.Trim());

        AppendCompanySizeEstabFilters(filters, parameters, request);

        if (request.HeadOfficeOnly)
        {
            filters.Add(request.ExcludeHeadOfficeOnly
                ? "e.identificador_matriz_filial <> '1'"
                : "e.identificador_matriz_filial = '1'");
        }
    }

    private static void AppendEmpresaSemiJoinFilter(
        List<string> estabFilters,
        List<NpgsqlParameter> parameters,
        CompanyFilterRequest request,
        bool skipLegalNatureAndCapital = false)
    {
        var empresaConditions = new List<string> { "c.execution_id = @execution_id" };
        var hasEmpresaFilter = false;

        if (!skipLegalNatureAndCapital)
        {
            if (HasValues(request.LegalNatureCodes))
            {
                hasEmpresaFilter = true;
                AppendStringArrayFilterToList(
                    empresaConditions, parameters, request.LegalNatureCodes, request.ExcludeLegalNatureCodes,
                    "c.natureza_juridica", "natureza", v => v.Trim());
            }

            if (AppendShareCapitalFilter(empresaConditions, parameters, request))
                hasEmpresaFilter = true;
        }

        if (!hasEmpresaFilter)
            return;

        var innerWhere = string.Join(" AND ", empresaConditions);
        estabFilters.Add(
            $"e.cnpj_basico IN (SELECT c.cnpj_basico FROM receita_empresa_staging c WHERE {innerWhere})");
    }

    private static void AppendCompanySizeEstabFilters(
        List<string> filters,
        List<NpgsqlParameter> parameters,
        CompanyFilterRequest request)
    {
        if (!HasValues(request.CompanySizeCodes))
            return;

        var codes = request.CompanySizeCodes!
            .Select(c => c.Trim())
            .Where(c => c.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var hasMei = codes.Contains("1");
        var porteCodes = codes
            .Where(c => CompanySizePorteMap.ContainsKey(c))
            .Select(c => CompanySizePorteMap[c])
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var parts = new List<string>();

        if (hasMei)
            parts.Add(request.ExcludeCompanySizes ? "e.opcao_mei IS DISTINCT FROM 'S'" : "e.opcao_mei = 'S'");

        if (porteCodes.Count > 0)
        {
            var porteIn = BuildInClause(porteCodes, "porte_estab", parameters, v => v);
            parts.Add(request.ExcludeCompanySizes
                ? $"e.cnpj_basico NOT IN (SELECT c.cnpj_basico FROM receita_empresa_staging c WHERE c.execution_id = @execution_id AND c.porte_empresa IN ({porteIn}))"
                : $"e.cnpj_basico IN (SELECT c.cnpj_basico FROM receita_empresa_staging c WHERE c.execution_id = @execution_id AND c.porte_empresa IN ({porteIn}))");
        }

        if (parts.Count == 0)
            return;

        var combined = request.ExcludeCompanySizes
            ? $"({string.Join(" AND ", parts)})"
            : $"({string.Join(" OR ", parts)})";

        filters.Add(combined);
    }

    private static bool AppendShareCapitalFilter(
        List<string> conditions,
        List<NpgsqlParameter> parameters,
        CompanyFilterRequest request,
        string tableAlias = "c")
    {
        if (!request.ShareCapitalMin.HasValue && !request.ShareCapitalMax.HasValue)
            return false;

        var capitalExpr = $"NULLIF(REPLACE({tableAlias}.capital_social, ',', '.'), '')::numeric";

        if (request.ExcludeShareCapitalRange)
        {
            var parts = new List<string>();
            if (request.ShareCapitalMin.HasValue && request.ShareCapitalMax.HasValue)
            {
                parts.Add($"({capitalExpr} < @capitalMin OR {capitalExpr} > @capitalMax OR {capitalExpr} IS NULL)");
                parameters.Add(new NpgsqlParameter("capitalMin", request.ShareCapitalMin.Value));
                parameters.Add(new NpgsqlParameter("capitalMax", request.ShareCapitalMax.Value));
            }
            else if (request.ShareCapitalMin.HasValue)
            {
                parts.Add($"({capitalExpr} < @capitalMin OR {capitalExpr} IS NULL)");
                parameters.Add(new NpgsqlParameter("capitalMin", request.ShareCapitalMin.Value));
            }
            else
            {
                parts.Add($"({capitalExpr} > @capitalMax OR {capitalExpr} IS NULL)");
                parameters.Add(new NpgsqlParameter("capitalMax", request.ShareCapitalMax!.Value));
            }

            conditions.Add(string.Join(" AND ", parts));
        }
        else
        {
            if (request.ShareCapitalMin.HasValue)
            {
                conditions.Add($"{capitalExpr} >= @capitalMin");
                parameters.Add(new NpgsqlParameter("capitalMin", request.ShareCapitalMin.Value));
            }

            if (request.ShareCapitalMax.HasValue)
            {
                conditions.Add($"{capitalExpr} <= @capitalMax");
                parameters.Add(new NpgsqlParameter("capitalMax", request.ShareCapitalMax.Value));
            }
        }

        return true;
    }

    private static void AppendStringArrayFilter(
        List<string> filters,
        List<NpgsqlParameter> parameters,
        string[]? values,
        bool exclude,
        string column,
        string paramPrefix,
        Func<string, string> normalize)
    {
        if (!HasValues(values))
            return;

        AppendStringArrayFilterToList(filters, parameters, values, exclude, column, paramPrefix, normalize);
    }

    private static void AppendStringArrayFilterToList(
        List<string> target,
        List<NpgsqlParameter> parameters,
        string[]? values,
        bool exclude,
        string column,
        string paramPrefix,
        Func<string, string> normalize)
    {
        if (!HasValues(values))
            return;

        var normalized = values!
            .Select(normalize)
            .Where(v => v.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
            return;

        var inClause = BuildInClause(normalized, paramPrefix, parameters, v => v);
        target.Add(exclude
            ? $"({column} NOT IN ({inClause}) OR {column} IS NULL)"
            : $"{column} IN ({inClause})");
    }

    private static string BuildInClause(
        IReadOnlyList<string> values,
        string paramPrefix,
        List<NpgsqlParameter> parameters,
        Func<string, string> normalize)
    {
        var placeholders = new List<string>();
        for (var i = 0; i < values.Count; i++)
        {
            var paramName = $"{paramPrefix}{i}";
            placeholders.Add($"@{paramName}");
            parameters.Add(new NpgsqlParameter(paramName, normalize(values[i])));
        }

        return string.Join(", ", placeholders);
    }

    private static bool HasValues(string[]? values) =>
        values is { Length: > 0 } && values.Any(v => !string.IsNullOrWhiteSpace(v));
}
