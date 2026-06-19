namespace RFBDataProject.Application.Services.Companies;

public sealed record CompanyDetailDto
{
    public required string Cnpj { get; init; }
    public required string LegalName { get; init; }
    public string? TradeName { get; init; }
    public string? LegalNatureCode { get; init; }
    public string? LegalNatureDescription { get; init; }
    public string? ShareCapital { get; init; }
    public string? CompanySizeCode { get; init; }
    public string? RegistrationStatus { get; init; }
    public string? ActivityStartDate { get; init; }
    public string? PrimaryCnaeCode { get; init; }
    public string? PrimaryCnaeDescription { get; init; }
    public string? StateCode { get; init; }
    public string? MunicipalityCode { get; init; }
    public string? MunicipalityDescription { get; init; }
    public string? StreetName { get; init; }
    public string? StreetNumber { get; init; }
    public string? Neighborhood { get; init; }
    public string? ZipCode { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
}
