using RFBDataProject.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Persistence.Repositories;

public sealed class PartnerRepository : IPartnerRepository
{
    private readonly ApplicationDbContext _context;

    public PartnerRepository(ApplicationDbContext context) => _context = context;

    public async Task<bool> ExistsAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM socios LIMIT 1)", conn);
        return (bool)(await cmd.ExecuteScalarAsync(ct) ?? false);
    }
}
