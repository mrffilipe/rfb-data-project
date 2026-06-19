using RFBDataProject.Domain.Rules;

namespace RFBDataProject.Domain.ValueObjects;

public sealed record Cnpj
{
    public string Value { get; private init; } = default!;

    public static Cnpj From(string value)
    {
        var digits = CnpjValidationRules.NormalizeDigits(value);
        CnpjValidationRules.ValidateCnpj(digits);
        return new Cnpj { Value = digits };
    }

    public CnpjBase ToBase() => CnpjBase.From(Value[..8]);

    public static implicit operator string(Cnpj value) => value.Value;
}
