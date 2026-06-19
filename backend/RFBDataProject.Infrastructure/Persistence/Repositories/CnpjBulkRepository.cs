using RFBDataProject.Domain.Repositories;
using RFBDataProject.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Persistence.Repositories;

public sealed class CnpjBulkRepository : ICnpjBulkRepository
{
    private readonly ApplicationDbContext _context;

    public CnpjBulkRepository(ApplicationDbContext context) => _context = context;

    public async Task TruncateAllCnpjTablesAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(CnpjSchemaSql.TruncateAll, conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task CreateIndexesAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(CnpjSchemaSql.CreateBtreeIndexes, conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task CreateSearchIndexesAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(CnpjSchemaSql.CreateSearchIndexes, conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task CreateViewsAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(CnpjSchemaSql.CreateViews, conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(CnpjSchemaSql.CreateTables, conn);
        await cmd.ExecuteNonQueryAsync(ct);

        await CreateViewsAsync(ct);
    }
}
