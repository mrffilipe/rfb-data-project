namespace RFBDataProject.Domain.Repositories;

public interface ICnpjBulkRepository
{
    Task EnsureSchemaAsync(CancellationToken ct = default);
    Task TruncateAllCnpjTablesAsync(CancellationToken ct = default);
    Task CreateIndexesAsync(CancellationToken ct = default);
    Task CreateSearchIndexesAsync(CancellationToken ct = default);
    Task CreateViewsAsync(CancellationToken ct = default);
}
