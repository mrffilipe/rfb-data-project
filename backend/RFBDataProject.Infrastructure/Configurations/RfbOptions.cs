namespace RFBDataProject.Infrastructure.Configurations;

public sealed record RfbOptions
{
    public const string SECTION = "Rfb";
    public required IReadOnlyList<string> BaseUrls { get; init; }
    public int DownloadParallelism { get; init; } = 4;
    public int SyncIntervalHours { get; init; } = 24;
    public bool RunSyncOnStartup { get; init; } = true;
    public int MaxDownloadRetries { get; init; } = 5;
    public string UserAgent { get; init; } = "RFBDataProject/1.0";
}
