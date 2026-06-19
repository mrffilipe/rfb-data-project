using RFBDataProject.Application.Exceptions;
using RFBDataProject.Application.Services.Ingestion;
using RFBDataProject.Application.Services.Rfb;
using RFBDataProject.Application.Services.UnitOfWork;
using RFBDataProject.Domain.Entities;
using RFBDataProject.Domain.Enums;
using RFBDataProject.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace RFBDataProject.Infrastructure.Services.Ingestion;

public sealed class IngestionService : IIngestionService
{
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private static volatile bool IsRunning;

    private readonly IRfbDiscoveryService _discovery;
    private readonly IRfbDownloadService _download;
    private readonly ICnpjBulkLoader _bulkLoader;
    private readonly ICnpjBulkRepository _bulkRepository;
    private readonly IIngestionReleaseRepository _releaseRepository;
    private readonly IIngestionRunRepository _runRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IngestionService> _logger;
    private readonly Configurations.RfbOptions _options;

    public IngestionService(
        IRfbDiscoveryService discovery,
        IRfbDownloadService download,
        ICnpjBulkLoader bulkLoader,
        ICnpjBulkRepository bulkRepository,
        IIngestionReleaseRepository releaseRepository,
        IIngestionRunRepository runRepository,
        IUnitOfWork unitOfWork,
        Microsoft.Extensions.Options.IOptions<Configurations.RfbOptions> options,
        ILogger<IngestionService> logger)
    {
        _discovery = discovery;
        _download = download;
        _bulkLoader = bulkLoader;
        _bulkRepository = bulkRepository;
        _releaseRepository = releaseRepository;
        _runRepository = runRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IngestionStatusDto> GetStatusAsync(CancellationToken ct = default)
    {
        var active = await _releaseRepository.GetActiveReleaseAsync(ct);
        var latest = await _releaseRepository.GetLatestReleaseAsync(ct);
        var latestRun = await _runRepository.GetLatestRunAsync(ct);
        var release = latest ?? active;

        var artifacts = release?.Artifacts.ToList() ?? [];
        return new IngestionStatusDto
        {
            IsSyncRunning = IsRunning,
            IsDataReady = active is not null,
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
                await RunSyncAsync(CancellationToken.None);
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

        try
        {
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
            await _unitOfWork.SaveChangesAsync(ct);

            await _bulkRepository.EnsureSchemaAsync(ct);

            var shouldTruncate = active is null || active.ReferencePeriod != discovery.ReferencePeriod;
            if (shouldTruncate)
                await _bulkRepository.TruncateAllCnpjTablesAsync(ct);

            var semaphore = new SemaphoreSlim(_options.DownloadParallelism);
            var tasks = discovery.Artifacts.Select(async remote =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    await ProcessArtifactAsync(release, remote, shouldTruncate, ct);
                    run.RecordArtifactProcessed();
                }
                catch (Exception ex)
                {
                    run.RecordArtifactFailed();
                    _logger.LogError(ex, "Failed to process artifact {FileName}", remote.FileName);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

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
                await _unitOfWork.SaveChangesAsync(ct);
                return;
            }

            if (active is not null && active.Id != reloaded.Id)
            {
                active.MarkSuperseded();
                _releaseRepository.Update(active);
            }

            await _bulkRepository.CreateIndexesAsync(ct);
            await _bulkRepository.CreateSearchIndexesAsync(ct);
            await _bulkRepository.CreateViewsAsync(ct);

            reloaded.MarkActive();
            _releaseRepository.Update(reloaded);
            run.Complete();
            _runRepository.Update(run);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Ingestion completed for reference period {ReferencePeriod}.", discovery.ReferencePeriod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ingestion sync failed.");
            if (run is not null)
            {
                run.Fail(ex.Message);
                _runRepository.Update(run);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            throw;
        }
        finally
        {
            IsRunning = false;
            SyncLock.Release();
        }
    }

    private async Task ProcessArtifactAsync(
        IngestionRelease release,
        RfbRemoteArtifact remote,
        bool forceReload,
        CancellationToken ct)
    {
        var artifact = release.Artifacts.First(a => a.FileName == remote.FileName);
        var remoteSize = await _download.GetRemoteSizeAsync(remote.DownloadUrl, ct);

        if (!forceReload && artifact.IsAlreadyLoaded(remoteSize))
        {
            artifact.MarkSkipped();
            _releaseRepository.Update(release);
            await _unitOfWork.SaveChangesAsync(ct);
            return;
        }

        artifact.MarkDownloading();
        _releaseRepository.Update(release);
        await _unitOfWork.SaveChangesAsync(ct);

        await using var zipStream = await _download.DownloadToTempFileStreamAsync(remote.DownloadUrl, ct);
        await using var csvStream = await Rfb.RfbZipCsvStreamReader.OpenSingleCsvEntryAsync(zipStream, ct);
        await _bulkLoader.LoadCsvStreamAsync(remote.TargetTable, csvStream, ct);

        artifact.MarkLoaded(remoteSize);
        _releaseRepository.Update(release);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
