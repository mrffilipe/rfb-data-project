namespace RFBDataProject.Application.Services.Lookups;

public sealed record LookupItemDto
{
    public required string Code { get; init; }
    public required string Description { get; init; }
}
