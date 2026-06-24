using Microsoft.Extensions.Caching.Memory;
using RFBDataProject.Application.Exceptions;
using RFBDataProject.Domain.Repositories;

namespace RFBDataProject.Infrastructure.Services.Staging;

public interface IStagingExecutionResolver
{
    Task<Guid> GetActiveExecutionIdAsync(CancellationToken ct = default);
}

public sealed class StagingExecutionResolver : IStagingExecutionResolver
{
    private const string CacheKey = "staging:active-execution-id";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IIngestionReleaseRepository _releaseRepository;
    private readonly ICnpjBulkRepository _bulkRepository;
    private readonly IMemoryCache _cache;

    public StagingExecutionResolver(
        IIngestionReleaseRepository releaseRepository,
        ICnpjBulkRepository bulkRepository,
        IMemoryCache cache)
    {
        _releaseRepository = releaseRepository;
        _bulkRepository = bulkRepository;
        _cache = cache;
    }

    public async Task<Guid> GetActiveExecutionIdAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue<Guid>(CacheKey, out var cached))
            return cached;

        var release = await _releaseRepository.GetActiveReleaseAsync(ct)
            ?? await _releaseRepository.GetLatestReleaseAsync(ct)
            ?? throw new ApplicationValidationException(ApplicationErrorMessages.Ingestion.NO_RELEASE_AVAILABLE);

        var executionId = await _bulkRepository.FindLatestStagingExecutionIdAsync(release.ReferencePeriod, ct)
            ?? throw new ApplicationValidationException(ApplicationErrorMessages.Ingestion.DATA_NOT_READY);

        _cache.Set(CacheKey, executionId, CacheDuration);
        return executionId;
    }
}
