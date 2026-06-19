namespace RFBDataProject.Application.Services.Rfb;

public sealed record RfbDiscoveryResult
{
    public required string ReferencePeriod { get; init; }
    public required string BaseUrl { get; init; }
    public required IReadOnlyList<RfbRemoteArtifact> Artifacts { get; init; }
}
