using RFBDataProject.Domain.Constants;
using RFBDataProject.Domain.Repositories;
using RFBDataProject.Infrastructure.Ingestion.Persistence;
using RFBDataProject.Infrastructure.Persistence.Sql;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace RFBDataProject.Infrastructure.Persistence.Repositories;

public sealed class CnpjBulkRepository : ICnpjBulkRepository
{
    private readonly INpgsqlBulkConnectionFactory _connections;
    private readonly ILogger<CnpjBulkRepository> _logger;

    public CnpjBulkRepository(INpgsqlBulkConnectionFactory connections, ILogger<CnpjBulkRepository> logger)
    {
        _connections = connections;
        _logger = logger;
    }

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Ensuring staging tables exist.");
        await using var conn = await _connections.OpenAsync(ct);
        await ExecuteAsync(conn, StagingSchemaSql.CreateTables, ct);
        _logger.LogInformation("Staging tables ensured.");
    }

    public async Task DropDomainTablesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Dropping legacy domain tables if present.");
        await using var conn = await _connections.OpenAsync(ct);
        await ExecuteAsync(conn, StagingSchemaSql.DropDomainTables, ct);
        _logger.LogInformation("Legacy domain tables dropped.");
    }

    public async Task ClearStagingForExecutionAsync(Guid executionId, CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);

        foreach (var table in StagingTableNames.All)
        {
            await using var cmd = _connections.CreateCommand(conn, StagingSchemaSql.ClearExecution(table));
            cmd.Parameters.AddWithValue("execution_id", executionId);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

    public async Task CreateStagingIndexesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Creating staging btree indexes.");
        await using var conn = await _connections.OpenAsync(ct);
        await ExecuteAsync(conn, StagingSchemaSql.CreateIndexes, ct);
        _logger.LogInformation("Staging btree indexes ensured.");
    }

    public async Task CreateStagingFilterIndexesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Creating staging filter indexes (may take several minutes on large datasets).");
        await using var conn = await _connections.OpenAsync(ct);
        await ExecuteAsync(conn, StagingSchemaSql.CreateFilterIndexes, ct);
        _logger.LogInformation("Staging filter indexes ensured.");
    }

    public async Task CreateStagingSearchIndexesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Creating staging text search indexes (may take several minutes on large datasets).");
        await using var conn = await _connections.OpenAsync(ct);
        await ExecuteAsync(conn, StagingSchemaSql.CreateSearchIndexes, ct);
        _logger.LogInformation("Staging text search indexes ensured.");
    }

    public async Task<Guid?> FindLatestStagingExecutionIdAsync(string referencePeriod, CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);

        const string sql = """
            SELECT ie.id
            FROM import_executions ie
            INNER JOIN (
                SELECT execution_id, COUNT(*) AS row_count
                FROM receita_empresa_staging
                GROUP BY execution_id
            ) st ON st.execution_id = ie.id
            WHERE ie.referencia_receita = @reference_period
              AND st.row_count > 0
            ORDER BY st.row_count DESC
            LIMIT 1
            """;

        await using var cmd = _connections.CreateCommand(conn, sql);
        cmd.Parameters.AddWithValue("reference_period", referencePeriod);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result is Guid id ? id : null;
    }

    private static async Task ExecuteAsync(NpgsqlConnection conn, string sql, CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(sql, conn) { CommandTimeout = 0 };
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
