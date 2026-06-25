namespace RFBDataProject.Application.Services.Companies;

public sealed record ExportCompaniesRequest : CompanyFilterRequest
{
    public int Limit { get; init; } = 100;
    public bool DeduplicateEmail { get; init; }
}
