using RFBDataProject.Application.Exceptions;
using RFBDataProject.Application.Services.Ingestion;
using RFBDataProject.Application.Services.Rfb;
using RFBDataProject.Application.Services.UnitOfWork;
using RFBDataProject.Domain.Entities;
using RFBDataProject.Domain.Enums;
using RFBDataProject.Domain.Repositories;
using RFBDataProject.Infrastructure.Ingestion.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RFBDataProject.Infrastructure.Services.Ingestion;

public sealed class IngestionService : IIngestionService
{
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private static volatile bool IsRunning;

    private readonly IRfbDiscoveryService _discovery;
    private readonly IIngestionPipelineOrchestrator _orchestrator;
    private readonly ICnpjBulkRepository _bulkRepository;
    private readonly IIngestionReleaseRepository _releaseRepository;
    private readonly IIngestionRunRepository _runRepository;
    private readonly IImportExecutionRepository _importExecutionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IngestionService> _logger;

    public IngestionService(
        IRfbDiscoveryService discovery,
        IIngestionPipelineOrchestrator orchestrator,
        ICnpjBulkRepository bulkRepository,
        IIngestionReleaseRepository releaseRepository,
        IIngestionRunRepository runRepository,
        IImportExecutionRepository importExecutionRepository,
        IUnitOfWork unitOfWork,
        IServiceScopeFactory scopeFactory,
        ILogger<IngestionService> logger)
    {
        _discovery = discovery;
        _orchestrator = orchestrator;
        _bulkRepository = bulkRepository;
        _releaseRepository = releaseRepository;
        _runRepository = runRepository;
        _importExecutionRepository = importExecutionRepository;
        _unitOfWork = unitOfWork;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<IngestionStatusDto> GetStatusAsync(CancellationToken ct = default)
    {
        var active = await _releaseRepository.GetActiveReleaseAsync(ct);
        var latest = await _releaseRepository.GetLatestReleaseAsync(ct);
        var latestRun = await _runRepository.GetLatestRunAsync(ct);
        var release = latest ?? active;
        var referencePeriod = active?.ReferencePeriod ?? latest?.ReferencePeriod;
        var hasStagingData = referencePeriod is not null
            && await _bulkRepository.FindLatestStagingExecutionIdAsync(referencePeriod, ct) is not null;

        var artifacts = release?.Artifacts.ToList() ?? [];
        return new IngestionStatusDto
        {
            IsSyncRunning = IsRunning,
            IsDataReady = hasStagingData,
            ActiveReferencePeriod = active?.ReferencePeriod,
            LatestReferencePeriod = latest?.ReferencePeriod,
            ReleaseStatus = release?.Status.ToString(),
            TotalArtifacts = artifacts.Count,
            LoadedArtifacts = artifacts.Count(a => a.Status == IngestionArtifactStatus.Loaded),
            FailedArtifacts = artifacts.Count(a => a.Status == IngestionArtifactStatus.Failed),
            LastSyncStartedAt = latestRun?.StartedAt,
            LastSyncCompletedAt = latestRun?.CompletedAt,
            LastError = latestRun?.ErrorMessage ?? release?.ErrorMessage,
            Artifacts = artifacts.Select(a => new IngestionArtifactStatusDto
            {
                FileName = a.FileName,
                TargetTable = a.TargetTable,
                Status = a.Status.ToString(),
                RemoteSize = a.RemoteSize,
                LoadedAt = a.LoadedAt
            }).ToList()
        };
    }

    public Task TriggerSyncAsync(CancellationToken ct = default)
    {
        if (IsRunning)
            throw new ApplicationValidationException(ApplicationErrorMessages.Ingestion.SYNC_ALREADY_RUNNING);

        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var ingestion = scope.ServiceProvider.GetRequiredService<IIngestionService>();
                await ingestion.RunSyncAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background ingestion sync failed.");
            }
        }, CancellationToken.None);

        return Task.CompletedTask;
    }

    public async Task RunSyncAsync(CancellationToken ct = default)
    {
        if (!await SyncLock.WaitAsync(0, ct))
            throw new ApplicationValidationException(ApplicationErrorMessages.Ingestion.SYNC_ALREADY_RUNNING);

        IsRunning = true;
        IngestionRun? run = null;
        ImportExecution? execution = null;

        try
        {
            await CleanupInterruptedRunsAsync(ct);

            var discovery = await _discovery.DiscoverLatestAsync(ct);
            if (discovery is null)
            {
                _logger.LogWarning("No RFB release discovered.");
                return;
            }

            var active = await _releaseRepository.GetActiveReleaseAsync(ct);
            if (active?.ReferencePeriod == discovery.ReferencePeriod &&
                active.Status == IngestionReleaseStatus.Active)
            {
                _logger.LogInformation("Reference period {ReferencePeriod} is already active.", discovery.ReferencePeriod);
                return;
            }

            var release = await _releaseRepository.GetByReferencePeriodAsync(discovery.ReferencePeriod, ct);
            if (release is null)
            {
                release = IngestionRelease.Create(discovery.ReferencePeriod, discovery.BaseUrl);
                foreach (var artifact in discovery.Artifacts)
                    release.AddArtifact(artifact.FileName, artifact.TargetTable, artifact.RemoteSize);
                await _releaseRepository.AddAsync(release, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            release.MarkInProgress();
            _releaseRepository.Update(release);
            await _unitOfWork.SaveChangesAsync(ct);

            run = IngestionRun.Start(release.Id);
            await _runRepository.AddAsync(run, ct);

            execution = ImportExecution.Start(run.Id, release.ReferencePeriod);
            await _importExecutionRepository.AddAsync(execution, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var stagingComplete = release.Artifacts.All(a =>
                a.Status is IngestionArtifactStatus.Loaded or IngestionArtifactStatus.Skipped);

            Guid? reuseStagingExecutionId = null;
            if (stagingComplete)
            {
                reuseStagingExecutionId = await _bulkRepository.FindLatestStagingExecutionIdAsync(
                    release.ReferencePeriod, ct);
                if (reuseStagingExecutionId is not null)
                {
                    _logger.LogInformation(
                        "Staging complete for {ReferencePeriod}. Resuming from existing staging without re-download.",
                        release.ReferencePeriod);
                }
            }

            var forceReload = !stagingComplete || reuseStagingExecutionId is null;

            await _orchestrator.RunAsync(
                release,
                run,
                execution,
                discovery.Artifacts,
                forceReload,
                reuseStagingExecutionId,
                ct);

            var reloaded = await _releaseRepository.GetByIdAsync(release.Id, ct)
                ?? throw new InvalidOperationException("Release not found after sync.");

            var allLoaded = reloaded.Artifacts.All(a =>
                a.Status is IngestionArtifactStatus.Loaded or IngestionArtifactStatus.Skipped);

            if (!allLoaded)
            {
                reloaded.MarkFailed("One or more artifacts failed to load.");
                _releaseRepository.Update(reloaded);
                run.Fail("One or more artifacts failed to load.");
                _runRepository.Update(run);
                execution.Fail("One or more artifacts failed to load.");
                _importExecutionRepository.Update(execution);
                await _unitOfWork.SaveChangesAsync(ct);
                return;
            }

            if (active is not null && active.Id != reloaded.Id)
            {
                active.MarkSuperseded();
                _releaseRepository.Update(active);
            }

            reloaded.MarkActive();
            _releaseRepository.Update(reloaded);
            run.Complete();
            _runRepository.Update(run);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Ingestion completed for {ReferencePeriod}. Processed={Processed}, Inserted={Inserted}, Updated={Updated}, Ignored={Ignored}",
                discovery.ReferencePeriod,
                execution.ProcessedCount,
                execution.InsertedCount,
                execution.UpdatedCount,
                execution.IgnoredCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ingestion sync failed.");
            if (run is not null)
            {
                run.Fail(ex.Message);
                _runRepository.Update(run);
            }

            if (execution is not null)
            {
                execution.Fail(ex.Message);
                _importExecutionRepository.Update(execution);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            throw;
        }
        finally
        {
            IsRunning = false;
            SyncLock.Release();
        }
    }

    private async Task CleanupInterruptedRunsAsync(CancellationToken ct)
    {
        var threshold = DateTime.UtcNow.AddHours(-2);
        var staleRuns = await _runRepository.GetStaleRunningAsync(threshold, ct);
        if (staleRuns.Count == 0)
            return;

        foreach (var staleRun in staleRuns)
        {
            staleRun.Fail("Interrupted by restart");
            _runRepository.Update(staleRun);

            var staleExecution = await _importExecutionRepository.GetByRunIdAsync(staleRun.Id, ct);
            if (staleExecution is { Status: ImportExecutionStatus.Running })
            {
                staleExecution.Fail("Interrupted by restart");
                _importExecutionRepository.Update(staleExecution);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Marked {Count} interrupted ingestion run(s) as failed.", staleRuns.Count);
    }
}
