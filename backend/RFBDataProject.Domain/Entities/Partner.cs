using RFBDataProject.Domain.Common;
using RFBDataProject.Domain.Exceptions;
using RFBDataProject.Domain.Rules;

namespace RFBDataProject.Domain.Entities;

public sealed class Partner : BaseEntity
{
    public string CnpjBase { get; private set; } = default!;
    public string? PartnerTypeIdentifier { get; private set; }
    public string PartnerName { get; private set; } = default!;
    public string? PartnerDocument { get; private set; }
    public string? PartnerQualificationCode { get; private set; }
    public string? EntryDate { get; private set; }
    public string? CountryCode { get; private set; }
    public string? LegalRepresentativeIdentifier { get; private set; }
    public string? LegalRepresentativeName { get; private set; }
    public string? LegalRepresentativeQualificationCode { get; private set; }
    public string? AgeRangeCode { get; private set; }

    private Partner()
    {
    }

    public static Partner Create(
        string cnpjBase,
        string partnerName,
        string? partnerTypeIdentifier = null,
        string? partnerDocument = null,
        string? partnerQualificationCode = null,
        string? entryDate = null,
        string? countryCode = null,
        string? legalRepresentativeIdentifier = null,
        string? legalRepresentativeName = null,
        string? legalRepresentativeQualificationCode = null,
        string? ageRangeCode = null)
    {
        CnpjValidationRules.ValidateCnpjBase(cnpjBase);
        if (string.IsNullOrWhiteSpace(partnerName))
            throw new DomainValidationException(DomainErrorMessages.Partner.PARTNER_NAME_REQUIRED);

        var partner = new Partner
        {
            CnpjBase = CnpjValidationRules.NormalizeDigits(cnpjBase),
            PartnerTypeIdentifier = partnerTypeIdentifier,
            PartnerName = partnerName.Trim(),
            PartnerDocument = partnerDocument,
            PartnerQualificationCode = partnerQualificationCode,
            EntryDate = entryDate,
            CountryCode = countryCode,
            LegalRepresentativeIdentifier = legalRepresentativeIdentifier,
            LegalRepresentativeName = legalRepresentativeName,
            LegalRepresentativeQualificationCode = legalRepresentativeQualificationCode,
            AgeRangeCode = ageRangeCode
        };
        partner.SetCreatedAt();
        return partner;
    }
}
