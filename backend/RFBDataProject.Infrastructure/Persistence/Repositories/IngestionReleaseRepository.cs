using RFBDataProject.Domain.Entities;
using RFBDataProject.Domain.Enums;
using RFBDataProject.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace RFBDataProject.Infrastructure.Persistence.Repositories;

public sealed class IngestionReleaseRepository : IIngestionReleaseRepository
{
    private readonly ApplicationDbContext _context;

    public IngestionReleaseRepository(ApplicationDbContext context) => _context = context;

    public Task<IngestionRelease?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.IngestionReleases
            .Include(x => x.Artifacts)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<IngestionRelease?> GetByReferencePeriodAsync(string referencePeriod, CancellationToken ct = default) =>
        _context.IngestionReleases
            .Include(x => x.Artifacts)
            .FirstOrDefaultAsync(x => x.ReferencePeriod == referencePeriod, ct);

    public Task<IngestionRelease?> GetActiveReleaseAsync(CancellationToken ct = default) =>
        _context.IngestionReleases
            .Include(x => x.Artifacts)
            .Where(x => x.Status == IngestionReleaseStatus.Active)
            .OrderByDescending(x => x.CompletedAt)
            .FirstOrDefaultAsync(ct);

    public Task<IngestionRelease?> GetLatestReleaseAsync(CancellationToken ct = default) =>
        _context.IngestionReleases
            .Include(x => x.Artifacts)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(IngestionRelease release, CancellationToken ct = default) =>
        await _context.IngestionReleases.AddAsync(release, ct);

    public void Update(IngestionRelease release) => _context.IngestionReleases.Update(release);
}
