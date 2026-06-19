using RFBDataProject.Application.Common;
using RFBDataProject.Application.Exceptions;
using RFBDataProject.Application.Services.Participations;
using RFBDataProject.Domain.Rules;
using RFBDataProject.Infrastructure.Persistence;
using RFBDataProject.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Services.Participations;

public sealed class CorporateParticipationQueryService : ICorporateParticipationQueryService
{
    private readonly ApplicationDbContext _context;

    public CorporateParticipationQueryService(ApplicationDbContext context) => _context = context;

    public async Task<PagedResult<CorporateParticipationDto>> ListAsync(
        ListCorporateParticipationsRequest request,
        CancellationToken ct = default)
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

        if (!string.IsNullOrWhiteSpace(request.ControllingCnpj))
        {
            filters.Add("cnpj_controladora = @controladora");
            parameters.Add(new NpgsqlParameter("controladora", CnpjValidationRules.NormalizeDigits(request.ControllingCnpj)));
        }

        if (!string.IsNullOrWhiteSpace(request.ControlledCnpj))
        {
            filters.Add("cnpj_controlada_basico = @controlada");
            parameters.Add(new NpgsqlParameter("controlada", CnpjValidationRules.NormalizeDigits(request.ControlledCnpj)[..8]));
        }

        var where = filters.Count > 0 ? "WHERE " + string.Join(" AND ", filters) : string.Empty;
        var countSql = $"SELECT COUNT(*) FROM vw_participacoes_pj {where}";
        var sql = $"""
            SELECT cnpj_controlada_basico, cnpj_controladora, razao_controladora,
                   qualificacao_socio, data_entrada_sociedade, razao_controlada
            FROM vw_participacoes_pj
            {where}
            ORDER BY cnpj_controladora, cnpj_controlada_basico
            LIMIT @limit OFFSET @offset
            """;

        parameters.Add(new NpgsqlParameter("limit", request.PageSize));
        parameters.Add(new NpgsqlParameter("offset", offset));

        await using var countCmd = new NpgsqlCommand(countSql, conn);
        NpgsqlCommandBinder.AddParameters(countCmd, parameters, excludePaging: true);
        var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct) ?? 0);

        await using var cmd = new NpgsqlCommand(sql, conn);
        NpgsqlCommandBinder.AddParameters(cmd, parameters);

        var items = new List<CorporateParticipationDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new CorporateParticipationDto
            {
                ControlledCnpjBase = reader.GetString(0),
                ControllingCnpj = reader.GetString(1),
                ControllingLegalName = reader.IsDBNull(2) ? null : reader.GetString(2),
                PartnerQualificationCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                EntryDate = reader.IsDBNull(4) ? null : reader.GetString(4),
                ControlledLegalName = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return new PagedResult<CorporateParticipationDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }
}
