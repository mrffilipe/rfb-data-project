namespace RFBDataProject.Infrastructure.Ingestion.Pipeline;

public sealed record RegistroReceita(
    string TargetTable,
    string[] Fields,
    Guid ExecutionId);
