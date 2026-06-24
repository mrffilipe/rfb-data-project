using RFBDataProject.Domain.Constants;

namespace RFBDataProject.Domain.Constants;

public static class StagingTableNames
{
    public const string Empresa = "receita_empresa_staging";
    public const string Estabelecimento = "receita_estabelecimento_staging";
    public const string Socio = "receita_socio_staging";
    public const string Simples = "receita_simples_staging";
    public const string Cnae = "receita_cnae_staging";
    public const string Motivo = "receita_motivo_staging";
    public const string Municipio = "receita_municipio_staging";
    public const string Natureza = "receita_natureza_staging";
    public const string Pais = "receita_pais_staging";
    public const string Qualificacao = "receita_qualificacao_staging";

    public static readonly IReadOnlyList<string> All =
    [
        Empresa, Estabelecimento, Socio, Simples,
        Cnae, Motivo, Municipio, Natureza, Pais, Qualificacao
    ];

    public static string FromTargetTable(string targetTable) => targetTable switch
    {
        RfbTableNames.Companies => Empresa,
        RfbTableNames.Establishments => Estabelecimento,
        RfbTableNames.Partners => Socio,
        RfbTableNames.SimplesNational => Simples,
        RfbTableNames.Cnaes => Cnae,
        RfbTableNames.Reasons => Motivo,
        RfbTableNames.Municipalities => Municipio,
        RfbTableNames.LegalNatures => Natureza,
        RfbTableNames.Countries => Pais,
        RfbTableNames.Qualifications => Qualificacao,
        _ => throw new ArgumentException($"Unknown target table: {targetTable}", nameof(targetTable))
    };

    public static string ToTargetTable(string stagingTable) => stagingTable switch
    {
        Empresa => RfbTableNames.Companies,
        Estabelecimento => RfbTableNames.Establishments,
        Socio => RfbTableNames.Partners,
        Simples => RfbTableNames.SimplesNational,
        Cnae => RfbTableNames.Cnaes,
        Motivo => RfbTableNames.Reasons,
        Municipio => RfbTableNames.Municipalities,
        Natureza => RfbTableNames.LegalNatures,
        Pais => RfbTableNames.Countries,
        Qualificacao => RfbTableNames.Qualifications,
        _ => throw new ArgumentException($"Unknown staging table: {stagingTable}", nameof(stagingTable))
    };
}
