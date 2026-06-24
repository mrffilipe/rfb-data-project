namespace RFBDataProject.Application.Services.Companies;

public abstract record CompanyFilterRequest : Common.PagedRequest
{
    public string? Query { get; init; }
    public string? StateCode { get; init; }
    public string? Cnae { get; init; }
    public string? LegalNatureCode { get; init; }
    public string? CompanySizeCode { get; init; }
    public string? RegistrationStatus { get; init; }
    public bool HeadOfficeOnly { get; init; }
    public decimal? ShareCapitalMin { get; init; }
    public decimal? ShareCapitalMax { get; init; }
}
