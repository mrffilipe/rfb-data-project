using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using RFBDataProject.Application.Services.Rfb;
using RFBDataProject.Application.Services.UnitOfWork;
using RFBDataProject.Domain.Entities;
using RFBDataProject.Domain.Repositories;
using RFBDataProject.Infrastructure.Configurations;
using RFBDataProject.Infrastructure.Ingestion.Metrics;
using RFBDataProject.Infrastructure.Ingestion.Parsing;
using RFBDataProject.Infrastructure.Ingestion.Persistence;
using RFBDataProject.Infrastructure.Rfb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RFBDataProject.Infrastructure.Ingestion.Pipeline;

public interface IIngestionPipelineOrchestrator
{
    Task RunAsync(
        IngestionRelease release,
        IngestionRun run,
        ImportExecution execution,
        IReadOnlyList<RfbRemoteArtifact> artifacts,
        bool forceReload,
        Guid? reuseStagingExecutionId = null,
        CancellationToken ct = default);
}

public sealed class IngestionPipelineOrchestrator : IIngestionPipelineOrchestrator
{
    private readonly IRfbDownloadService _download;
    private readonly IStagingBulkWriter _stagingWriter;
    private readonly ICnpjBulkRepository _schemaRepository;
    private readonly IImportExecutionRepository _importExecutionRepository;
    private readonly IIngestionReleaseRepository _releaseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PipelineChannelFactory _channelFactory;
    private readonly RfbOptions _options;
    private readonly RfbIngestionMetrics _metrics;
    private readonly ILogger<IngestionPipelineOrchestrator> _logger;
    private readonly SemaphoreSlim _dbGate = new(1, 1);

    public IngestionPipelineOrchestrator(
        IRfbDownloadService download,
        IStagingBulkWriter stagingWriter,
        ICnpjBulkRepository schemaRepository,
        IImportExecutionRepository importExecutionRepository,
        IIngestionReleaseRepository releaseRepository,
        IUnitOfWork unitOfWork,
        PipelineChannelFactory channelFactory,
        IOptions<RfbOptions> options,
        RfbIngestionMetrics metrics,
        ILogger<IngestionPipelineOrchestrator> logger)
    {
        _download = download;
        _stagingWriter = stagingWriter;
        _schemaRepository = schemaRepository;
        _importExecutionRepository = importExecutionRepository;
        _releaseRepository = releaseRepository;
        _unitOfWork = unitOfWork;
        _channelFactory = channelFactory;
        _options = options.Value;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task RunAsync(
        IngestionRelease release,
        IngestionRun run,
        ImportExecution execution,
        IReadOnlyList<RfbRemoteArtifact> artifacts,
        bool forceReload,
        Guid? reuseStagingExecutionId = null,
        CancellationToken ct = default)
    {
        await _schemaRepository.EnsureSchemaAsync(ct);
        await _schemaRepository.DropDomainTablesAsync(ct);

        if (reuseStagingExecutionId is not null)
        {
            _logger.LogInformation(
                "Resuming from existing staging execution {StagingExecutionId}.",
                reuseStagingExecutionId.Value);
        }
        else
        {
            await _schemaRepository.ClearStagingForExecutionAsync(execution.Id, ct);
            await RunStagingPipelineAsync(release, execution, artifacts, forceReload, ct);
        }

        await _schemaRepository.CreateStagingIndexesAsync(ct);
        await _schemaRepository.CreateStagingFilterIndexesAsync(ct);
        await _schemaRepository.CreateStagingSearchIndexesAsync(ct);
        execution.Complete();
        _importExecutionRepository.Update(execution);
    }

    private async Task RunStagingPipelineAsync(
        IngestionRelease release,
        ImportExecution execution,
        IReadOnlyList<RfbRemoteArtifact> artifacts,
        bool forceReload,
        CancellationToken ct)
    {
        var registroChannel = _channelFactory.Create<RegistroReceita>();
        var tableSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        var stagingWriters = Enumerable.Range(0, _options.StagingWriterParallelism)
            .Select(_ => RunStagingWriterAsync(registroChannel.Reader, tableSemaphores, ct))
            .ToArray();

        var downloadSemaphore = new SemaphoreSlim(_options.DownloadParallelism);
        var downloadTasks = artifacts.Select(artifact => ProcessArtifactAsync(
            release, execution, artifact, forceReload, registroChannel.Writer, downloadSemaphore, ct)).ToArray();

        Exception? failure = null;
        try
        {
            await Task.WhenAll(downloadTasks);
        }
        catch (Exception ex)
        {
            failure = ex;
        }
        finally
        {
            registroChannel.Writer.TryComplete(failure);
        }

        await Task.WhenAll(stagingWriters);
        if (failure is not null)
            throw failure;
    }

    private async Task ProcessArtifactAsync(
        IngestionRelease release,
        ImportExecution execution,
        RfbRemoteArtifact remote,
        bool forceReload,
        ChannelWriter<RegistroReceita> output,
        SemaphoreSlim downloadSemaphore,
        CancellationToken ct)
    {
        var artifact = release.Artifacts.First(a => a.FileName == remote.FileName);
        var remoteSize = await _download.GetRemoteSizeAsync(remote.DownloadUrl, ct);

        if (!forceReload && artifact.IsAlreadyLoaded(remoteSize))
        {
            artifact.MarkSkipped();
            await PersistReleaseAsync(release, ct);
            return;
        }

        artifact.MarkDownloading();
        await PersistReleaseAsync(release, ct);

        await downloadSemaphore.WaitAsync(ct);
        try
        {
            var sw = Stopwatch.StartNew();
            var zipStream = await _download.DownloadAsStreamAsync(remote.DownloadUrl, ct);
            using var csvStream = RfbZipEntryStream.OpenFirstCsvEntry(zipStream);
            sw.Stop();
            _metrics.RecordDownload(remoteSize ?? 0, sw.Elapsed.TotalSeconds);

            await foreach (var fields in RfbCsvParser.ParseAsync(csvStream, ct))
            {
                _metrics.RecordParsedRows(1);
                execution.RecordProcessed();
                await output.WriteAsync(
                    new RegistroReceita(remote.TargetTable, fields, execution.Id), ct);
            }

            artifact.MarkLoaded(remoteSize);
            await PersistReleaseAsync(release, ct);
            _logger.LogInformation("Loaded artifact {FileName} into staging.", remote.FileName);
        }
        catch (Exception ex)
        {
            artifact.MarkFailed(ex.Message);
            await PersistReleaseAsync(release, ct);
            throw;
        }
        finally
        {
            downloadSemaphore.Release();
        }
    }

    private async Task PersistReleaseAsync(IngestionRelease release, CancellationToken ct)
    {
        await _dbGate.WaitAsync(ct);
        try
        {
            _releaseRepository.Update(release);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        finally
        {
            _dbGate.Release();
        }
    }

    private async Task RunStagingWriterAsync(
        ChannelReader<RegistroReceita> input,
        ConcurrentDictionary<string, SemaphoreSlim> tableSemaphores,
        CancellationToken ct)
    {
        var buffers = new Dictionary<string, (Guid ExecutionId, List<string[]> Rows)>();

        await foreach (var registro in input.ReadAllAsync(ct))
        {
            if (!buffers.TryGetValue(registro.TargetTable, out var entry))
                entry = (registro.ExecutionId, new List<string[]>(_options.StagingBatchSize));

            entry.Rows.Add(registro.Fields);
            buffers[registro.TargetTable] = entry;

            if (entry.Rows.Count >= _options.StagingBatchSize)
            {
                await FlushBufferAsync(registro.TargetTable, entry.ExecutionId, entry.Rows, tableSemaphores, ct);
                buffers[registro.TargetTable] = (entry.ExecutionId, new List<string[]>(_options.StagingBatchSize));
            }
        }

        foreach (var (table, entry) in buffers)
        {
            if (entry.Rows.Count > 0)
                await FlushBufferAsync(table, entry.ExecutionId, entry.Rows, tableSemaphores, ct);
        }
    }

    private async Task FlushBufferAsync(
        string targetTable,
        Guid executionId,
        List<string[]> buffer,
        ConcurrentDictionary<string, SemaphoreSlim> tableSemaphores,
        CancellationToken ct)
    {
        var rows = buffer.ToArray();
        buffer.Clear();
        if (rows.Length == 0)
            return;

        var semaphore = tableSemaphores.GetOrAdd(targetTable, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        try
        {
            await _stagingWriter.WriteBatchAsync(targetTable, executionId, rows, ct);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
