namespace RFBDataProject.Application.Services.Partners;

public sealed record PartnerDto
{
    public required string CnpjBase { get; init; }
    public required string PartnerName { get; init; }
    public string? PartnerTypeIdentifier { get; init; }
    public string? PartnerDocument { get; init; }
    public string? PartnerQualificationCode { get; init; }
    public string? EntryDate { get; init; }
}
