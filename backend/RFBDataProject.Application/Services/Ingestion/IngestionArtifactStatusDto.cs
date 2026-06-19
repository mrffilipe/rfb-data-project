namespace RFBDataProject.Application.Services.Ingestion;

public sealed record IngestionArtifactStatusDto
{
    public required string FileName { get; init; }
    public required string TargetTable { get; init; }
    public required string Status { get; init; }
    public long? RemoteSize { get; init; }
    public DateTime? LoadedAt { get; init; }
}
