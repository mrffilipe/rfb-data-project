using RFBDataProject.Domain.Entities;
using RFBDataProject.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace RFBDataProject.Infrastructure.Persistence.Repositories;

public sealed class ImportExecutionRepository : IImportExecutionRepository
{
    private readonly ApplicationDbContext _context;

    public ImportExecutionRepository(ApplicationDbContext context) => _context = context;

    public Task<ImportExecution?> GetByRunIdAsync(Guid runId, CancellationToken ct = default) =>
        _context.ImportExecutions.FirstOrDefaultAsync(x => x.RunId == runId, ct);

    public async Task AddAsync(ImportExecution execution, CancellationToken ct = default) =>
        await _context.ImportExecutions.AddAsync(execution, ct);

    public void Update(ImportExecution execution) => _context.ImportExecutions.Update(execution);
}
