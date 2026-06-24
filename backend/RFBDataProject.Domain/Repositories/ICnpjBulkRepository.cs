namespace RFBDataProject.Domain.Repositories;

public interface ICnpjBulkRepository
{
    Task EnsureSchemaAsync(CancellationToken ct = default);
    Task DropDomainTablesAsync(CancellationToken ct = default);
    Task ClearStagingForExecutionAsync(Guid executionId, CancellationToken ct = default);
    Task CreateStagingIndexesAsync(CancellationToken ct = default);
    Task CreateStagingFilterIndexesAsync(CancellationToken ct = default);
    Task CreateStagingSearchIndexesAsync(CancellationToken ct = default);
    Task<Guid?> FindLatestStagingExecutionIdAsync(string referencePeriod, CancellationToken ct = default);
}
