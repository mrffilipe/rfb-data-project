using RFBDataProject.Domain.Entities;

namespace RFBDataProject.Domain.Repositories;

public interface IIngestionReleaseRepository
{
    Task<IngestionRelease?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IngestionRelease?> GetByReferencePeriodAsync(string referencePeriod, CancellationToken ct = default);
    Task<IngestionRelease?> GetActiveReleaseAsync(CancellationToken ct = default);
    Task<IngestionRelease?> GetLatestReleaseAsync(CancellationToken ct = default);
    Task AddAsync(IngestionRelease release, CancellationToken ct = default);
    void Update(IngestionRelease release);
}
