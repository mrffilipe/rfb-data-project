namespace RFBDataProject.Application.Services.Partners;

public sealed record GetCompaniesByPartnerRequest : Common.PagedRequest
{
    public required string Document { get; init; }
}
