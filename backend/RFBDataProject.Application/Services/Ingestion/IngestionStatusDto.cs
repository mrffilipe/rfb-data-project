namespace RFBDataProject.Application.Services.Ingestion;

public sealed record IngestionStatusDto
{
    public bool IsSyncRunning { get; init; }
    public bool IsDataReady { get; init; }
    public string? ActiveReferencePeriod { get; init; }
    public string? LatestReferencePeriod { get; init; }
    public string? ReleaseStatus { get; init; }
    public int TotalArtifacts { get; init; }
    public int LoadedArtifacts { get; init; }
    public int FailedArtifacts { get; init; }
    public DateTime? LastSyncStartedAt { get; init; }
    public DateTime? LastSyncCompletedAt { get; init; }
    public string? LastError { get; init; }
    public IReadOnlyList<IngestionArtifactStatusDto> Artifacts { get; init; } = [];
}
