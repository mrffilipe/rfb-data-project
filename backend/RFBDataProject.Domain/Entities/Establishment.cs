using RFBDataProject.Domain.Common;
using RFBDataProject.Domain.Exceptions;
using RFBDataProject.Domain.Rules;

namespace RFBDataProject.Domain.Entities;

public sealed class Establishment : BaseEntity
{
    public string CnpjBase { get; private set; } = default!;
    public string CnpjOrder { get; private set; } = default!;
    public string CnpjCheckDigit { get; private set; } = default!;
    public string? HeadOfficeBranchIdentifier { get; private set; }
    public string? TradeName { get; private set; }
    public string? RegistrationStatus { get; private set; }
    public string? RegistrationStatusDate { get; private set; }
    public string? RegistrationStatusReasonCode { get; private set; }
    public string? ForeignCityName { get; private set; }
    public string? CountryCode { get; private set; }
    public string? ActivityStartDate { get; private set; }
    public string? PrimaryCnaeCode { get; private set; }
    public string? SecondaryCnaeCodes { get; private set; }
    public string? StreetType { get; private set; }
    public string? StreetName { get; private set; }
    public string? StreetNumber { get; private set; }
    public string? AddressComplement { get; private set; }
    public string? Neighborhood { get; private set; }
    public string? ZipCode { get; private set; }
    public string? StateCode { get; private set; }
    public string? MunicipalityCode { get; private set; }
    public string? PhoneAreaCode1 { get; private set; }
    public string? PhoneNumber1 { get; private set; }
    public string? PhoneAreaCode2 { get; private set; }
    public string? PhoneNumber2 { get; private set; }
    public string? FaxAreaCode { get; private set; }
    public string? FaxNumber { get; private set; }
    public string? Email { get; private set; }
    public string? SpecialStatus { get; private set; }
    public string? SpecialStatusDate { get; private set; }

    public string FullCnpj => CnpjBase + CnpjOrder + CnpjCheckDigit;

    private Establishment()
    {
    }

    public static Establishment Create(
        string cnpjBase,
        string cnpjOrder,
        string cnpjCheckDigit,
        string? headOfficeBranchIdentifier = null,
        string? tradeName = null,
        string? registrationStatus = null,
        string? registrationStatusDate = null,
        string? registrationStatusReasonCode = null,
        string? foreignCityName = null,
        string? countryCode = null,
        string? activityStartDate = null,
        string? primaryCnaeCode = null,
        string? secondaryCnaeCodes = null,
        string? streetType = null,
        string? streetName = null,
        string? streetNumber = null,
        string? addressComplement = null,
        string? neighborhood = null,
        string? zipCode = null,
        string? stateCode = null,
        string? municipalityCode = null,
        string? phoneAreaCode1 = null,
        string? phoneNumber1 = null,
        string? phoneAreaCode2 = null,
        string? phoneNumber2 = null,
        string? faxAreaCode = null,
        string? faxNumber = null,
        string? email = null,
        string? specialStatus = null,
        string? specialStatusDate = null)
    {
        CnpjValidationRules.ValidateCnpjBase(cnpjBase);
        if (string.IsNullOrWhiteSpace(cnpjOrder))
            throw new DomainValidationException(DomainErrorMessages.Establishment.CNPJ_ORDER_REQUIRED);
        if (string.IsNullOrWhiteSpace(cnpjCheckDigit))
            throw new DomainValidationException(DomainErrorMessages.Establishment.CNPJ_CHECK_DIGIT_REQUIRED);

        if (!string.IsNullOrWhiteSpace(stateCode))
            CnpjValidationRules.ValidateStateCode(stateCode);

        var establishment = new Establishment
        {
            CnpjBase = CnpjValidationRules.NormalizeDigits(cnpjBase),
            CnpjOrder = cnpjOrder.Trim(),
            CnpjCheckDigit = cnpjCheckDigit.Trim(),
            HeadOfficeBranchIdentifier = headOfficeBranchIdentifier,
            TradeName = tradeName,
            RegistrationStatus = registrationStatus,
            RegistrationStatusDate = registrationStatusDate,
            RegistrationStatusReasonCode = registrationStatusReasonCode,
            ForeignCityName = foreignCityName,
            CountryCode = countryCode,
            ActivityStartDate = activityStartDate,
            PrimaryCnaeCode = primaryCnaeCode,
            SecondaryCnaeCodes = secondaryCnaeCodes,
            StreetType = streetType,
            StreetName = streetName,
            StreetNumber = streetNumber,
            AddressComplement = addressComplement,
            Neighborhood = neighborhood,
            ZipCode = zipCode,
            StateCode = stateCode?.Trim().ToUpperInvariant(),
            MunicipalityCode = municipalityCode,
            PhoneAreaCode1 = phoneAreaCode1,
            PhoneNumber1 = phoneNumber1,
            PhoneAreaCode2 = phoneAreaCode2,
            PhoneNumber2 = phoneNumber2,
            FaxAreaCode = faxAreaCode,
            FaxNumber = faxNumber,
            Email = email,
            SpecialStatus = specialStatus,
            SpecialStatusDate = specialStatusDate
        };
        establishment.SetCreatedAt();
        return establishment;
    }
}
