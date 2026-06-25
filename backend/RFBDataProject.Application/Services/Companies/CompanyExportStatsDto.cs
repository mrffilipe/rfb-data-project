namespace RFBDataProject.Application.Services.Companies;

public sealed record CompanyExportStatsDto
{
    public int RequestedLimit { get; init; }
    public int ExportedCount { get; init; }
    public int ScannedCount { get; init; }
    public int WithEmailCount { get; init; }
    public int WithoutEmailCount { get; init; }
    public int UniqueEmailCount { get; init; }
    public int DuplicateEmailSkippedCount { get; init; }
}
