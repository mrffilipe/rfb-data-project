using RFBDataProject.Domain.Common;
using RFBDataProject.Domain.Enums;
using RFBDataProject.Domain.Exceptions;
using RFBDataProject.Domain.Rules;

namespace RFBDataProject.Domain.Entities;

public sealed class IngestionRelease : BaseEntity
{
    public string ReferencePeriod { get; private set; } = default!;
    public IngestionReleaseStatus Status { get; private set; }
    public string? BaseUrl { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public ICollection<IngestionArtifact> Artifacts { get; private set; } = [];

    private IngestionRelease()
    {
    }

    public static IngestionRelease Create(string referencePeriod, string? baseUrl = null)
    {
        CnpjValidationRules.ValidateReferencePeriod(referencePeriod);

        var release = new IngestionRelease
        {
            ReferencePeriod = referencePeriod.Trim(),
            Status = IngestionReleaseStatus.Discovered,
            BaseUrl = baseUrl
        };
        release.SetCreatedAt();
        return release;
    }

    public void MarkInProgress()
    {
        if (Status is IngestionReleaseStatus.Active or IngestionReleaseStatus.Superseded)
            throw new DomainBusinessRuleException(DomainErrorMessages.IngestionRelease.INVALID_STATUS_TRANSITION);

        Status = IngestionReleaseStatus.InProgress;
        StartedAt ??= DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void MarkActive()
    {
        Status = IngestionReleaseStatus.Active;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = null;
        SetUpdatedAt();
    }

    public void MarkSuperseded()
    {
        Status = IngestionReleaseStatus.Superseded;
        SetUpdatedAt();
    }

    public void MarkFailed(string errorMessage)
    {
        Status = IngestionReleaseStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public IngestionArtifact AddArtifact(string fileName, string targetTable, long? remoteSize = null)
    {
        var artifact = IngestionArtifact.Create(Id, fileName, targetTable, remoteSize);
        Artifacts.Add(artifact);
        SetUpdatedAt();
        return artifact;
    }
}
