using RFBDataProject.Domain.Constants;

namespace RFBDataProject.Infrastructure.Ingestion;

public static class RfbCsvFileMapper
{
    private static readonly (string Token, string Table)[] Mappings =
    [
        ("ESTABELECIMENTOS", RfbTableNames.Establishments),
        ("ESTABELE", RfbTableNames.Establishments),
        ("EMPRESAS", RfbTableNames.Companies),
        ("EMPRE", RfbTableNames.Companies),
        ("SOCIOS", RfbTableNames.Partners),
        ("SOCIO", RfbTableNames.Partners),
        ("SIMPLES", RfbTableNames.SimplesNational),
        ("QUALIFICACOES", RfbTableNames.Qualifications),
        ("QUALS", RfbTableNames.Qualifications),
        ("NATUREZAS", RfbTableNames.LegalNatures),
        ("NATJU", RfbTableNames.LegalNatures),
        ("MUNICIPIOS", RfbTableNames.Municipalities),
        ("MUNIC", RfbTableNames.Municipalities),
        ("MOTIVOS", RfbTableNames.Reasons),
        ("MOTI", RfbTableNames.Reasons),
        ("PAISES", RfbTableNames.Countries),
        ("PAIS", RfbTableNames.Countries),
        ("CNAES", RfbTableNames.Cnaes),
        ("CNAE", RfbTableNames.Cnaes)
    ];

    public static string? MapFileNameToTable(string fileName)
    {
        var upper = fileName.ToUpperInvariant();
        foreach (var (token, table) in Mappings)
        {
            if (upper.Contains(token, StringComparison.Ordinal))
                return table;
        }

        return null;
    }
}
