using RFBDataProject.Domain.Entities;
using RFBDataProject.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace RFBDataProject.Infrastructure.Persistence.Repositories;

public sealed class IngestionRunRepository : IIngestionRunRepository
{
    private readonly ApplicationDbContext _context;

    public IngestionRunRepository(ApplicationDbContext context) => _context = context;

    public Task<IngestionRun?> GetLatestRunAsync(CancellationToken ct = default) =>
        _context.IngestionRuns
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(IngestionRun run, CancellationToken ct = default) =>
        await _context.IngestionRuns.AddAsync(run, ct);

    public void Update(IngestionRun run) => _context.IngestionRuns.Update(run);
}
