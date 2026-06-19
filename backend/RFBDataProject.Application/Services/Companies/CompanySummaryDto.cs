namespace RFBDataProject.Application.Services.Companies;

public sealed record CompanySummaryDto
{
    public required string Cnpj { get; init; }
    public required string LegalName { get; init; }
    public string? TradeName { get; init; }
    public string? StateCode { get; init; }
    public string? Municipality { get; init; }
    public string? PrimaryCnaeCode { get; init; }
    public string? PrimaryCnaeDescription { get; init; }
    public string? RegistrationStatus { get; init; }
}
