namespace RFBDataProject.Domain.Enums;

public enum IngestionArtifactStatus
{
    Pending = 0,
    Downloading = 1,
    Loaded = 2,
    Failed = 3,
    Skipped = 4
}
