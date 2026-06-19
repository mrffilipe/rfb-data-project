using RFBDataProject.Application.Common;
using RFBDataProject.Application.Exceptions;
using RFBDataProject.Application.Services.Companies;
using RFBDataProject.Domain.Rules;
using RFBDataProject.Infrastructure.Persistence;
using RFBDataProject.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Services.Companies;

public sealed class CompanyQueryService : ICompanyQueryService
{
    private readonly ApplicationDbContext _context;

    public CompanyQueryService(ApplicationDbContext context) => _context = context;

    public async Task<CompanyDetailDto?> GetByCnpjAsync(GetCompanyByCnpjRequest request, CancellationToken ct = default)
    {
        var cnpj = CnpjValidationRules.NormalizeDigits(request.Cnpj);
        CnpjValidationRules.ValidateCnpj(cnpj);

        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            """
            SELECT cnpj, razao_social, nome_fantasia, natureza_juridica, natureza_juridica_desc,
                   capital_social, porte_empresa, situacao_cadastral, data_inicio_atividade,
                   cnae_fiscal_principal, cnae_principal_desc, uf, municipio, municipio_desc,
                   logradouro, numero, bairro, cep, correio_eletronico, telefone_1
            FROM vw_empresas_completo WHERE cnpj = @cnpj LIMIT 1
            """, conn);
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
            StateCode = S(11),
            MunicipalityCode = S(12),
            MunicipalityDescription = S(13),
            StreetName = S(14),
            StreetNumber = S(15),
            Neighborhood = S(16),
            ZipCode = S(17),
            Email = S(18),
            PhoneNumber = S(19)
        };
    }

    public async Task<PagedResult<CompanySummaryDto>> ListHoldingsAsync(ListHoldingsRequest request, CancellationToken ct = default)
    {
        if (request.Page < 1)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.INVALID_PAGE);
        if (request.PageSize is < 1 or > 100)
            throw new ApplicationValidationException(ApplicationErrorMessages.Search.INVALID_PAGE_SIZE);

        var offset = (request.Page - 1) * request.PageSize;
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        var filters = new List<string>();
        var parameters = new List<NpgsqlParameter>();
        if (!string.IsNullOrWhiteSpace(request.StateCode))
        {
            filters.Add("h.uf = @uf");
            parameters.Add(new NpgsqlParameter("uf", request.StateCode.Trim().ToUpperInvariant()));
        }

        var where = filters.Count > 0 ? "WHERE " + string.Join(" AND ", filters) : string.Empty;
        var countSql = $"SELECT COUNT(*) FROM vw_holdings h {where}";
        var sql = $"""
            SELECT h.cnpj, h.razao_social, NULL, h.uf, m.descricao, h.cnae_fiscal_principal, NULL, NULL
            FROM vw_holdings h
            LEFT JOIN municipios m ON m.codigo = h.municipio
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
