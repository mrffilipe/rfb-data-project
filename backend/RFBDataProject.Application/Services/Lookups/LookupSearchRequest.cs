namespace RFBDataProject.Application.Services.Lookups;

public sealed record LookupSearchRequest : Common.PagedRequest
{
    public string? Query { get; init; }
}
