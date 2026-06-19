namespace RFBDataProject.Domain.Constants;

public static class RfbArtifactDefinitions
{
    public sealed record ArtifactDefinition(string Prefix, int Parts, string TargetTable);

    public static readonly IReadOnlyList<ArtifactDefinition> All =
    [
        new("Empresas", 10, RfbTableNames.Companies),
        new("Estabelecimentos", 10, RfbTableNames.Establishments),
        new("Socios", 10, RfbTableNames.Partners),
        new("Simples", 1, RfbTableNames.SimplesNational),
        new("Cnaes", 1, RfbTableNames.Cnaes),
        new("Motivos", 1, RfbTableNames.Reasons),
        new("Municipios", 1, RfbTableNames.Municipalities),
        new("Naturezas", 1, RfbTableNames.LegalNatures),
        new("Paises", 1, RfbTableNames.Countries),
        new("Qualificacoes", 1, RfbTableNames.Qualifications)
    ];

    public static IEnumerable<string> ExpectedZipFileNames()
    {
        foreach (var definition in All)
        {
            if (definition.Parts == 1)
            {
                yield return $"{definition.Prefix}.zip";
                continue;
            }

            for (var i = 0; i < definition.Parts; i++)
                yield return $"{definition.Prefix}{i}.zip";
        }
    }
}
