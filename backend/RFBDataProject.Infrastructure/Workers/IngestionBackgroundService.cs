using RFBDataProject.Application.Services.Ingestion;
using RFBDataProject.Infrastructure.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RFBDataProject.Infrastructure.Workers;

public sealed class IngestionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RfbOptions _options;
    private readonly ILogger<IngestionBackgroundService> _logger;

    public IngestionBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<RfbOptions> options,
        ILogger<IngestionBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.RunSyncOnStartup)
            await RunSafeSyncAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(_options.SyncIntervalHours));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await RunSafeSyncAsync(stoppingToken);
    }

    private async Task RunSafeSyncAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var ingestion = scope.ServiceProvider.GetRequiredService<IIngestionService>();
            await ingestion.RunSyncAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled ingestion sync failed.");
        }
    }
}
