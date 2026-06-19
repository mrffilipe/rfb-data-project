namespace RFBDataProject.Application.Services.Companies;

public sealed record ListHoldingsRequest : Common.PagedRequest
{
    public string? StateCode { get; init; }
}
