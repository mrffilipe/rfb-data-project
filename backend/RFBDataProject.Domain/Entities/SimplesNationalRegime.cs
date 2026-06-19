using RFBDataProject.Domain.Common;
using RFBDataProject.Domain.Rules;

namespace RFBDataProject.Domain.Entities;

public sealed class SimplesNationalRegime : BaseEntity
{
    public string CnpjBase { get; private set; } = default!;
    public string? SimplesOption { get; private set; }
    public string? SimplesOptionDate { get; private set; }
    public string? SimplesExclusionDate { get; private set; }
    public string? MeiOption { get; private set; }
    public string? MeiOptionDate { get; private set; }
    public string? MeiExclusionDate { get; private set; }

    private SimplesNationalRegime()
    {
    }

    public static SimplesNationalRegime Create(
        string cnpjBase,
        string? simplesOption = null,
        string? simplesOptionDate = null,
        string? simplesExclusionDate = null,
        string? meiOption = null,
        string? meiOptionDate = null,
        string? meiExclusionDate = null)
    {
        CnpjValidationRules.ValidateCnpjBase(cnpjBase);

        var regime = new SimplesNationalRegime
        {
            CnpjBase = CnpjValidationRules.NormalizeDigits(cnpjBase),
            SimplesOption = simplesOption,
            SimplesOptionDate = simplesOptionDate,
            SimplesExclusionDate = simplesExclusionDate,
            MeiOption = meiOption,
            MeiOptionDate = meiOptionDate,
            MeiExclusionDate = meiExclusionDate
        };
        regime.SetCreatedAt();
        return regime;
    }
}
