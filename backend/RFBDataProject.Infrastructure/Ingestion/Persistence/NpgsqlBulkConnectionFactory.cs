using Microsoft.EntityFrameworkCore;
using Npgsql;
using RFBDataProject.Infrastructure.Persistence;

namespace RFBDataProject.Infrastructure.Ingestion.Persistence;

public interface INpgsqlBulkConnectionFactory
{
    Task<NpgsqlConnection> OpenAsync(CancellationToken ct = default);

    NpgsqlCommand CreateCommand(NpgsqlConnection conn, string sql);
}

public sealed class NpgsqlBulkConnectionFactory : INpgsqlBulkConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlBulkConnectionFactory(ApplicationDbContext context)
    {
        _connectionString = context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Database connection string is not configured.");
    }

    public async Task<NpgsqlConnection> OpenAsync(CancellationToken ct = default)
    {
        var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }

    public NpgsqlCommand CreateCommand(NpgsqlConnection conn, string sql)
    {
        var cmd = new NpgsqlCommand(sql, conn) { CommandTimeout = 0 };
        return cmd;
    }
}
