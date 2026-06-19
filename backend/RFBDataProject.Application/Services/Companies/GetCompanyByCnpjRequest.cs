namespace RFBDataProject.Application.Services.Companies;

public sealed record GetCompanyByCnpjRequest
{
    public required string Cnpj { get; init; }
}
