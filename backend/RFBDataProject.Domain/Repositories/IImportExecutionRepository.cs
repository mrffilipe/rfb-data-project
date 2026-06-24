using RFBDataProject.Domain.Entities;

namespace RFBDataProject.Domain.Repositories;

public interface IImportExecutionRepository
{
    Task<ImportExecution?> GetByRunIdAsync(Guid runId, CancellationToken ct = default);
    Task AddAsync(ImportExecution execution, CancellationToken ct = default);
    void Update(ImportExecution execution);
}
