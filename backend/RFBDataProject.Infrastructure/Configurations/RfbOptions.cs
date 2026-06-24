namespace RFBDataProject.Infrastructure.Configurations;

public sealed record RfbOptions
{
    public const string SECTION = "Rfb";
    public required IReadOnlyList<string> BaseUrls { get; init; }
    public int DownloadParallelism { get; init; } = 4;
    public int ParserParallelism { get; init; } = 4;
    public int StagingWriterParallelism { get; init; } = 2;
    public int ChannelCapacity { get; init; } = 10_000;
    public int StagingBatchSize { get; init; } = 5_000;
    public int SyncIntervalHours { get; init; } = 24;
    public bool RunSyncOnStartup { get; init; } = true;
    public int MaxDownloadRetries { get; init; } = 5;
    public string UserAgent { get; init; } = "RFBDataProject/1.0";
}
