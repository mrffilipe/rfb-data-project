namespace RFBDataProject.Application.Services.Participations;

public sealed record ListCorporateParticipationsRequest : Common.PagedRequest
{
    public string? ControllingCnpj { get; init; }
    public string? ControlledCnpj { get; init; }
}
