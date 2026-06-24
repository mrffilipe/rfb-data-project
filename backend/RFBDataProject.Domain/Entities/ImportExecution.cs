using RFBDataProject.Domain.Common;
using RFBDataProject.Domain.Enums;
using RFBDataProject.Domain.Exceptions;

namespace RFBDataProject.Domain.Entities;

public sealed class ImportExecution : BaseEntity
{
    public Guid RunId { get; private set; }
    public string ReferencePeriod { get; private set; } = default!;
    public ImportExecutionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public long ProcessedCount { get; private set; }
    public long InsertedCount { get; private set; }
    public long UpdatedCount { get; private set; }
    public long IgnoredCount { get; private set; }
    public string? Errors { get; private set; }

    private ImportExecution()
    {
    }

    public static ImportExecution Start(Guid runId, string referencePeriod)
    {
        if (runId == Guid.Empty)
            throw new DomainValidationException("Run id is required.");

        if (string.IsNullOrWhiteSpace(referencePeriod))
            throw new DomainValidationException("Reference period is required.");

        var execution = new ImportExecution
        {
            RunId = runId,
            ReferencePeriod = referencePeriod.Trim(),
            Status = ImportExecutionStatus.Running,
            StartedAt = DateTime.UtcNow
        };
        execution.SetCreatedAt();
        return execution;
    }

    public void RecordProcessed(long count = 1) => ProcessedCount += count;

    public void RecordInserted(long count = 1) => InsertedCount += count;

    public void RecordUpdated(long count = 1) => UpdatedCount += count;

    public void RecordIgnored(long count = 1) => IgnoredCount += count;

    public void Complete()
    {
        Status = ImportExecutionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Fail(string errorMessage)
    {
        Status = ImportExecutionStatus.Failed;
        Errors = errorMessage;
        CompletedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
