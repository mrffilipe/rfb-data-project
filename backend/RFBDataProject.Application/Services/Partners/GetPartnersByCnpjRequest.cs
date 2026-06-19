namespace RFBDataProject.Application.Services.Partners;

public sealed record GetPartnersByCnpjRequest : Common.PagedRequest
{
    public required string Cnpj { get; init; }
}
