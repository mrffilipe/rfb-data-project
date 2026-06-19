using RFBDataProject.Domain.Entities;

namespace RFBDataProject.Domain.Repositories;

public interface IIngestionRunRepository
{
    Task<IngestionRun?> GetLatestRunAsync(CancellationToken ct = default);
    Task AddAsync(IngestionRun run, CancellationToken ct = default);
    void Update(IngestionRun run);
}
