using RFBDataProject.Domain.Common;
using RFBDataProject.Domain.Exceptions;
using RFBDataProject.Domain.Rules;

namespace RFBDataProject.Domain.Entities;

public sealed class Company : BaseEntity
{
    public string CnpjBase { get; private set; } = default!;
    public string LegalName { get; private set; } = default!;
    public string? LegalNatureCode { get; private set; }
    public string? ResponsibleQualificationCode { get; private set; }
    public string? ShareCapital { get; private set; }
    public string? CompanySizeCode { get; private set; }
    public string? FederativeEntityResponsible { get; private set; }

    private Company()
    {
    }

    public static Company Create(
        string cnpjBase,
        string legalName,
        string? legalNatureCode = null,
        string? responsibleQualificationCode = null,
        string? shareCapital = null,
        string? companySizeCode = null,
        string? federativeEntityResponsible = null)
    {
        _ = ValueObjects.CnpjBase.From(cnpjBase);
        if (string.IsNullOrWhiteSpace(legalName))
            throw new DomainValidationException(DomainErrorMessages.Company.LEGAL_NAME_REQUIRED);

        var company = new Company
        {
            CnpjBase = CnpjValidationRules.NormalizeDigits(cnpjBase),
            LegalName = legalName.Trim(),
            LegalNatureCode = legalNatureCode,
            ResponsibleQualificationCode = responsibleQualificationCode,
            ShareCapital = shareCapital,
            CompanySizeCode = companySizeCode,
            FederativeEntityResponsible = federativeEntityResponsible
        };
        company.SetCreatedAt();
        return company;
    }
}
