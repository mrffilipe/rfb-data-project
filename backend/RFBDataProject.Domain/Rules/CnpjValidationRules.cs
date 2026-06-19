using RFBDataProject.Domain.Exceptions;

namespace RFBDataProject.Domain.Rules;

public static class CnpjValidationRules
{
    private static readonly int[] FirstWeight = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] SecondWeight = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    private static readonly HashSet<string> ValidStateCodes =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
        "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    public static string NormalizeDigits(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return new string(value.Where(char.IsDigit).ToArray());
    }

    public static void ValidateCnpjBase(string cnpjBase)
    {
        var digits = NormalizeDigits(cnpjBase);
        if (digits.Length == 0)
            throw new DomainValidationException(DomainErrorMessages.CnpjBase.REQUIRED);
        if (digits.Length != 8)
            throw new DomainValidationException(DomainErrorMessages.CnpjBase.INVALID_LENGTH);
    }

    public static void ValidateCnpj(string cnpj)
    {
        var digits = NormalizeDigits(cnpj);
        if (digits.Length == 0)
            throw new DomainValidationException(DomainErrorMessages.Cnpj.REQUIRED);
        if (digits.Length != 14)
            throw new DomainValidationException(DomainErrorMessages.Cnpj.INVALID_LENGTH);
        if (!IsValidCheckDigits(digits))
            throw new DomainValidationException(DomainErrorMessages.Cnpj.INVALID_CHECK_DIGITS);
    }

    public static void ValidateStateCode(string stateCode)
    {
        if (string.IsNullOrWhiteSpace(stateCode))
            throw new DomainValidationException(DomainErrorMessages.StateCode.REQUIRED);

        var normalized = stateCode.Trim().ToUpperInvariant();
        if (normalized.Length != 2)
            throw new DomainValidationException(DomainErrorMessages.StateCode.INVALID_LENGTH);
        if (!ValidStateCodes.Contains(normalized))
            throw new DomainValidationException(DomainErrorMessages.StateCode.INVALID_VALUE);
    }

    public static void ValidateReferencePeriod(string referencePeriod)
    {
        if (string.IsNullOrWhiteSpace(referencePeriod))
            throw new DomainValidationException(DomainErrorMessages.IngestionRelease.REFERENCE_PERIOD_REQUIRED);

        var trimmed = referencePeriod.Trim();
        var isYearMonth = trimmed.Length == 7 && trimmed[4] == '-' &&
                          int.TryParse(trimmed[..4], out _) && int.TryParse(trimmed[5..], out _);
        var isYearMonthDay = trimmed.Length == 10 && trimmed[4] == '-' && trimmed[7] == '-' &&
                             DateOnly.TryParse(trimmed, out _);

        if (!isYearMonth && !isYearMonthDay)
            throw new DomainValidationException(DomainErrorMessages.IngestionRelease.REFERENCE_PERIOD_INVALID_FORMAT);
    }

    private static bool IsValidCheckDigits(string digits)
    {
        if (digits.Distinct().Count() == 1)
            return false;

        var firstDigit = CalculateDigit(digits[..12], FirstWeight);
        if (digits[12] - '0' != firstDigit)
            return false;

        var secondDigit = CalculateDigit(digits[..13], SecondWeight);
        return digits[13] - '0' == secondDigit;
    }

    private static int CalculateDigit(string input, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
            sum += (input[i] - '0') * weights[i];

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
