using RFBDataProject.Application.Common;
using RFBDataProject.Application.Exceptions;
using RFBDataProject.Application.Services.Companies;
using RFBDataProject.Application.Services.Partners;
using RFBDataProject.Domain.Rules;
using RFBDataProject.Infrastructure.Persistence;
using RFBDataProject.Infrastructure.Services.Staging;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Services.Partners;

public sealed class PartnerQueryService : IPartnerQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly IStagingExecutionResolver _executionResolver;

    public PartnerQueryService(ApplicationDbContext context, IStagingExecutionResolver executionResolver)
    {
        _context = context;
        _executionResolver = executionResolver;
    }

    public async Task<PagedResult<CompanySummaryDto>> GetCompaniesByPartnerAsync(
        GetCompaniesByPartnerRequest request,
        CancellationToken ct = default)
    {
        ValidatePaged(request);
        var doc = CnpjValidationRules.NormalizeDigits(request.Document);
        if (string.IsNullOrEmpty(doc))
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.QUERY_TOO_SHORT);

        var executionId = await _executionResolver.GetActiveExecutionIdAsync(ct);
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        const string countSql = """
            SELECT COUNT(DISTINCT e.cnpj_basico || e.cnpj_ordem || e.cnpj_dv)
            FROM receita_socio_staging p
            JOIN receita_empresa_staging c
                ON c.cnpj_basico = p.cnpj_basico AND c.execution_id = p.execution_id
            JOIN receita_estabelecimento_staging e
                ON e.cnpj_basico = p.cnpj_basico
               AND e.execution_id = p.execution_id
               AND e.identificador_matriz_filial = '1'
            WHERE p.cnpj_cpf_socio = @doc
              AND p.execution_id = @execution_id
            """;

        const string sql = """
            SELECT DISTINCT e.cnpj_basico || e.cnpj_ordem || e.cnpj_dv,
                   c.razao_social, e.nome_fantasia, e.uf, m.descricao,
                   e.cnae_fiscal_principal, NULL, e.situacao_cadastral
            FROM receita_socio_staging p
            JOIN receita_empresa_staging c
                ON c.cnpj_basico = p.cnpj_basico AND c.execution_id = p.execution_id
            JOIN receita_estabelecimento_staging e
                ON e.cnpj_basico = p.cnpj_basico
               AND e.execution_id = p.execution_id
               AND e.identificador_matriz_filial = '1'
            LEFT JOIN receita_municipio_staging m
                ON m.codigo = e.municipio AND m.execution_id = p.execution_id
            WHERE p.cnpj_cpf_socio = @doc
              AND p.execution_id = @execution_id
            ORDER BY 1
            LIMIT @limit OFFSET @offset
            """;

        await using var countCmd = new NpgsqlCommand(countSql, conn);
        countCmd.Parameters.AddWithValue("doc", doc);
        countCmd.Parameters.AddWithValue("execution_id", executionId);
        var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct) ?? 0);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("doc", doc);
        cmd.Parameters.AddWithValue("execution_id", executionId);
        cmd.Parameters.AddWithValue("limit", request.PageSize);
        cmd.Parameters.AddWithValue("offset", (request.Page - 1) * request.PageSize);

        var items = await ReadCompanySummaryAsync(cmd, ct);
        return BuildPagedResult(items, request, total);
    }

    public async Task<PagedResult<PartnerDto>> GetPartnersByCnpjAsync(GetPartnersByCnpjRequest request, CancellationToken ct = default)
    {
        ValidatePaged(request);
        var cnpj = CnpjValidationRules.NormalizeDigits(request.Cnpj);
        CnpjValidationRules.ValidateCnpj(cnpj);
        var basico = cnpj[..8];
        var executionId = await _executionResolver.GetActiveExecutionIdAsync(ct);

        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        await using var countCmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM receita_socio_staging WHERE cnpj_basico = @basico AND execution_id = @execution_id",
            conn);
        countCmd.Parameters.AddWithValue("basico", basico);
        countCmd.Parameters.AddWithValue("execution_id", executionId);
        var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct) ?? 0);

        await using var cmd = new NpgsqlCommand(
            """
            SELECT cnpj_basico, nome_socio, identificador_socio, cnpj_cpf_socio,
                   qualificacao_socio, data_entrada_sociedade
            FROM receita_socio_staging
            WHERE cnpj_basico = @basico AND execution_id = @execution_id
            ORDER BY nome_socio
            LIMIT @limit OFFSET @offset
            """, conn);
        cmd.Parameters.AddWithValue("basico", basico);
        cmd.Parameters.AddWithValue("execution_id", executionId);
        cmd.Parameters.AddWithValue("limit", request.PageSize);
        cmd.Parameters.AddWithValue("offset", (request.Page - 1) * request.PageSize);

        var items = new List<PartnerDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new PartnerDto
            {
                CnpjBase = reader.GetString(0),
                PartnerName = reader.GetString(1),
                PartnerTypeIdentifier = reader.IsDBNull(2) ? null : reader.GetString(2),
                PartnerDocument = reader.IsDBNull(3) ? null : reader.GetString(3),
                PartnerQualificationCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                EntryDate = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return new PagedResult<PartnerDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

    private static async Task<List<CompanySummaryDto>> ReadCompanySummaryAsync(NpgsqlCommand cmd, CancellationToken ct)
    {
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
