using RFBDataProject.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RFBDataProject.Infrastructure.Ingestion;

public sealed class StagingIndexBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StagingIndexBackgroundService> _logger;

    public StagingIndexBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<StagingIndexBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICnpjBulkRepository>();
            await repository.CreateStagingFilterIndexesAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Background staging filter index creation failed.");
        }
    }
}
