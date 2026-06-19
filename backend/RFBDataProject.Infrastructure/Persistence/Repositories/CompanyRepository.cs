using RFBDataProject.Domain.Entities;
using RFBDataProject.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Persistence.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyRepository(ApplicationDbContext context) => _context = context;

    public async Task<Company?> GetByCnpjBaseAsync(string cnpjBase, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            """
            SELECT cnpj_basico, razao_social, natureza_juridica, qualificacao_responsavel,
                   capital_social, porte_empresa, ente_federativo_responsavel
            FROM empresas WHERE cnpj_basico = @cnpj LIMIT 1
            """, conn);
        cmd.Parameters.AddWithValue("cnpj", cnpjBase);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        return Company.Create(
            reader.GetString(0),
            reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6));
    }

    public async Task<bool> ExistsAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM empresas LIMIT 1)", conn);
        return (bool)(await cmd.ExecuteScalarAsync(ct) ?? false);
    }
}
