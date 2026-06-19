namespace RFBDataProject.Application.Services.Companies;

public sealed record ListCompaniesRequest : Common.PagedRequest
{
    public string? StateCode { get; init; }
    public string? Cnae { get; init; }
    public string? LegalNatureCode { get; init; }
    public bool HeadOfficeOnly { get; init; }
}
