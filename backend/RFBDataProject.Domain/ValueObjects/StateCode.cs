using RFBDataProject.Domain.Rules;

namespace RFBDataProject.Domain.ValueObjects;

public sealed record StateCode
{
    public string Value { get; private init; } = default!;

    public static StateCode From(string value)
    {
        CnpjValidationRules.ValidateStateCode(value);
        return new StateCode { Value = value.Trim().ToUpperInvariant() };
    }

    public static implicit operator string(StateCode value) => value.Value;
}
