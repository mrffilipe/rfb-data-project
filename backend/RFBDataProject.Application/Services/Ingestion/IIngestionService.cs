namespace RFBDataProject.Application.Services.Ingestion;

public interface IIngestionService
{
    Task<IngestionStatusDto> GetStatusAsync(CancellationToken ct = default);
    Task TriggerSyncAsync(CancellationToken ct = default);
    Task RunSyncAsync(CancellationToken ct = default);
}
