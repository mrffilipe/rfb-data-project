namespace RFBDataProject.Application.Services.Rfb;

public sealed record RfbRemoteArtifact
{
    public required string FileName { get; init; }
    public required string TargetTable { get; init; }
    public long? RemoteSize { get; init; }
    public required string DownloadUrl { get; init; }
}
