namespace RFBDataProject.Domain.Constants;

public static class RfbTableNames
{
    public const string Companies = "empresas";
    public const string Establishments = "estabelecimentos";
    public const string Partners = "socios";
    public const string SimplesNational = "simples";
    public const string Cnaes = "cnaes";
    public const string Reasons = "motivos";
    public const string Municipalities = "municipios";
    public const string LegalNatures = "naturezas";
    public const string Countries = "paises";
    public const string Qualifications = "qualificacoes";

    public static readonly IReadOnlyList<string> AllCnpjTables =
    [
        Companies, Establishments, Partners, SimplesNational,
        Cnaes, Reasons, Municipalities, LegalNatures, Countries, Qualifications
    ];
}
