using RFBDataProject.Domain.Common;
using RFBDataProject.Domain.Enums;
using RFBDataProject.Domain.Exceptions;

namespace RFBDataProject.Domain.Entities;

public sealed class IngestionArtifact : BaseEntity
{
    public Guid ReleaseId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string TargetTable { get; private set; } = default!;
    public long? RemoteSize { get; private set; }
    public string? Sha256 { get; private set; }
    public IngestionArtifactStatus Status { get; private set; }
    public DateTime? LoadedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    private IngestionArtifact()
    {
    }

    public static IngestionArtifact Create(Guid releaseId, string fileName, string targetTable, long? remoteSize = null)
    {
        if (releaseId == Guid.Empty)
            throw new DomainValidationException(DomainErrorMessages.IngestionRun.RELEASE_ID_REQUIRED);
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainValidationException(DomainErrorMessages.IngestionArtifact.FILE_NAME_REQUIRED);
        if (string.IsNullOrWhiteSpace(targetTable))
            throw new DomainValidationException(DomainErrorMessages.IngestionArtifact.TARGET_TABLE_REQUIRED);

        var artifact = new IngestionArtifact
        {
            ReleaseId = releaseId,
            FileName = fileName.Trim(),
            TargetTable = targetTable.Trim().ToLowerInvariant(),
            RemoteSize = remoteSize,
            Status = IngestionArtifactStatus.Pending
        };
        artifact.SetCreatedAt();
        return artifact;
    }

    public void MarkDownloading()
    {
        Status = IngestionArtifactStatus.Downloading;
        SetUpdatedAt();
    }

    public void MarkLoaded(long? remoteSize = null, string? sha256 = null)
    {
        Status = IngestionArtifactStatus.Loaded;
        RemoteSize = remoteSize ?? RemoteSize;
        Sha256 = sha256;
        LoadedAt = DateTime.UtcNow;
        ErrorMessage = null;
        SetUpdatedAt();
    }

    public void MarkFailed(string errorMessage)
    {
        Status = IngestionArtifactStatus.Failed;
        ErrorMessage = errorMessage;
        SetUpdatedAt();
    }

    public void MarkSkipped()
    {
        Status = IngestionArtifactStatus.Skipped;
        SetUpdatedAt();
    }

    public bool IsAlreadyLoaded(long? remoteSize) =>
        Status == IngestionArtifactStatus.Loaded &&
        RemoteSize.HasValue &&
        remoteSize.HasValue &&
        RemoteSize.Value == remoteSize.Value;
}
