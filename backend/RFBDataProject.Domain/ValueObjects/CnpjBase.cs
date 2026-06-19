using RFBDataProject.Domain.Rules;

namespace RFBDataProject.Domain.ValueObjects;

public sealed record CnpjBase
{
    public string Value { get; private init; } = default!;

    public static CnpjBase From(string value)
    {
        var digits = CnpjValidationRules.NormalizeDigits(value);
        CnpjValidationRules.ValidateCnpjBase(digits);
        return new CnpjBase { Value = digits };
    }

    public static implicit operator string(CnpjBase value) => value.Value;
}
