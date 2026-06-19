namespace RFBDataProject.Application.Services.Participations;

public sealed record CorporateParticipationDto
{
    public required string ControlledCnpjBase { get; init; }
    public required string ControllingCnpj { get; init; }
    public string? ControllingLegalName { get; init; }
    public string? PartnerQualificationCode { get; init; }
    public string? EntryDate { get; init; }
    public string? ControlledLegalName { get; init; }
}
