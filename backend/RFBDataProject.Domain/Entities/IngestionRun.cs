using RFBDataProject.Domain.Common;
using RFBDataProject.Domain.Enums;
using RFBDataProject.Domain.Exceptions;

namespace RFBDataProject.Domain.Entities;

public sealed class IngestionRun : BaseEntity
{
    public Guid ReleaseId { get; private set; }
    public IngestionRunStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int ArtifactsProcessed { get; private set; }
    public int ArtifactsFailed { get; private set; }

    private IngestionRun()
    {
    }

    public static IngestionRun Start(Guid releaseId)
    {
        if (releaseId == Guid.Empty)
            throw new DomainValidationException(DomainErrorMessages.IngestionRun.RELEASE_ID_REQUIRED);

        var run = new IngestionRun
        {
            ReleaseId = releaseId,
            Status = IngestionRunStatus.Running,
            StartedAt = DateTime.UtcNow
        };
        run.SetCreatedAt();
        return run;
    }

    public void RecordArtifactProcessed() => ArtifactsProcessed++;

    public void RecordArtifactFailed() => ArtifactsFailed++;

    public void Complete()
    {
        Status = IngestionRunStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Fail(string errorMessage)
    {
        Status = IngestionRunStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
