namespace RFBDataProject.Application.Services.Companies;

public sealed record CompanyExportResultDto
{
    public required IReadOnlyList<string> Columns { get; init; }
    public required CompanyExportStatsDto Stats { get; init; }
    public required IReadOnlyList<IReadOnlyDictionary<string, string?>> Items { get; init; }
}
