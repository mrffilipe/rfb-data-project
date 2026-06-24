using RFBDataProject.Infrastructure.Ingestion.Metrics;
using NpgsqlTypes;

namespace RFBDataProject.Infrastructure.Ingestion.Persistence;

public interface IStagingBulkWriter
{
    Task WriteBatchAsync(
        string targetTable,
        Guid executionId,
        IReadOnlyList<string[]> rows,
        CancellationToken ct = default);
}

public sealed class StagingBulkWriter : IStagingBulkWriter
{
    private readonly INpgsqlBulkConnectionFactory _connections;
    private readonly RfbIngestionMetrics _metrics;

    public StagingBulkWriter(INpgsqlBulkConnectionFactory connections, RfbIngestionMetrics metrics)
    {
        _connections = connections;
        _metrics = metrics;
    }

    public async Task WriteBatchAsync(
        string targetTable,
        Guid executionId,
        IReadOnlyList<string[]> rows,
        CancellationToken ct = default)
    {
        if (rows.Count == 0)
            return;

        var stagingTable = RfbTableColumns.GetStagingTable(targetTable);
        var columns = RfbTableColumns.GetStagingCopyColumns(targetTable);
        var columnList = string.Join(", ", columns.Select(c => $"\"{c}\""));

        await using var conn = await _connections.OpenAsync(ct);

        await using var importer = await conn.BeginBinaryImportAsync(
            $"COPY \"{stagingTable}\" ({columnList}) FROM STDIN (FORMAT BINARY)",
            ct);

        var dataColumns = RfbTableColumns.GetStagingDataColumns(targetTable);

        foreach (var row in rows)
        {
            await importer.StartRowAsync(ct);
            await importer.WriteAsync(executionId, NpgsqlDbType.Uuid, ct);

            for (var i = 0; i < dataColumns.Length; i++)
            {
                var value = i < row.Length ? row[i] : null;
                if (string.IsNullOrEmpty(value))
                    await importer.WriteNullAsync(ct);
                else
                    await importer.WriteAsync(value, NpgsqlDbType.Text, ct);
            }
        }

        await importer.CompleteAsync(ct);
        _metrics.RecordStagingRows(rows.Count);
    }
}
