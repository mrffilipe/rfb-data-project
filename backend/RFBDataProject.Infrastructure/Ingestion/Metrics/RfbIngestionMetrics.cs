using System.Diagnostics.Metrics;

namespace RFBDataProject.Infrastructure.Ingestion.Metrics;

public sealed class RfbIngestionMetrics
{
    public const string MeterName = "RFBDataProject.Ingestion";

    private readonly Counter<long> _downloadBytes;
    private readonly Histogram<double> _downloadDuration;
    private readonly Counter<long> _parseRows;
    private readonly Counter<long> _stagingRows;
    private readonly Counter<long> _diffChanged;
    private readonly Counter<long> _diffUnchanged;
    private readonly Histogram<double> _transformDuration;
    private readonly Counter<long> _upsertInserted;
    private readonly Counter<long> _upsertUpdated;

    public RfbIngestionMetrics()
    {
        var meter = new Meter(MeterName);
        _downloadBytes = meter.CreateCounter<long>("rfb.download.bytes");
        _downloadDuration = meter.CreateHistogram<double>("rfb.download.duration", unit: "s");
        _parseRows = meter.CreateCounter<long>("rfb.parse.rows");
        _stagingRows = meter.CreateCounter<long>("rfb.staging.rows_inserted");
        _diffChanged = meter.CreateCounter<long>("rfb.diff.changed");
        _diffUnchanged = meter.CreateCounter<long>("rfb.diff.unchanged");
        _transformDuration = meter.CreateHistogram<double>("rfb.transform.duration", unit: "s");
        _upsertInserted = meter.CreateCounter<long>("rfb.upsert.inserted");
        _upsertUpdated = meter.CreateCounter<long>("rfb.upsert.updated");
    }

    public void RecordDownload(long bytes, double seconds)
    {
        _downloadBytes.Add(bytes);
        _downloadDuration.Record(seconds);
    }

    public void RecordParsedRows(long count) => _parseRows.Add(count);

    public void RecordStagingRows(long count) => _stagingRows.Add(count);

    public void RecordDiffChanged(long count) => _diffChanged.Add(count);

    public void RecordDiffUnchanged(long count) => _diffUnchanged.Add(count);

    public void RecordTransform(double seconds) => _transformDuration.Record(seconds);

    public void RecordUpsertInserted(long count) => _upsertInserted.Add(count);

    public void RecordUpsertUpdated(long count) => _upsertUpdated.Add(count);
}
