using RFBDataProject.Domain.Entities;
using RFBDataProject.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Persistence.Repositories;

public sealed class EstablishmentRepository : IEstablishmentRepository
{
    private readonly ApplicationDbContext _context;

    public EstablishmentRepository(ApplicationDbContext context) => _context = context;

    public async Task<Establishment?> GetByCnpjAsync(string cnpj, CancellationToken ct = default)
    {
        if (cnpj.Length != 14)
            return null;

        var basico = cnpj[..8];
        var ordem = cnpj[8..12];
        var dv = cnpj[12..];

        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            """
            SELECT cnpj_basico, cnpj_ordem, cnpj_dv, identificador_matriz_filial, nome_fantasia,
                   situacao_cadastral, data_situacao_cadastral, motivo_situacao_cadastral,
                   nome_cidade_exterior, pais, data_inicio_atividade, cnae_fiscal_principal,
                   cnae_fiscal_secundaria, tipo_logradouro, logradouro, numero, complemento,
                   bairro, cep, uf, municipio, ddd_1, telefone_1, ddd_2, telefone_2,
                   ddd_fax, fax, correio_eletronico, situacao_especial, data_situacao_especial
            FROM estabelecimentos
            WHERE cnpj_basico = @basico AND cnpj_ordem = @ordem AND cnpj_dv = @dv
            LIMIT 1
            """, conn);
        cmd.Parameters.AddWithValue("basico", basico);
        cmd.Parameters.AddWithValue("ordem", ordem);
        cmd.Parameters.AddWithValue("dv", dv);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        string? S(int i) => reader.IsDBNull(i) ? null : reader.GetString(i);

        return Establishment.Create(
            reader.GetString(0), reader.GetString(1), reader.GetString(2),
            S(3), S(4), S(5), S(6), S(7), S(8), S(9), S(10), S(11), S(12),
            S(13), S(14), S(15), S(16), S(17), S(18), S(19), S(20),
            S(21), S(22), S(23), S(24), S(25), S(26), S(27), S(28), S(29));
    }
}
