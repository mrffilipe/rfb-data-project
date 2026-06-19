namespace RFBDataProject.Domain.Constants;

public static class LookupCatalogs
{
    public sealed record LookupEntry(string Code, string Description);

    public static readonly IReadOnlyList<LookupEntry> BrazilianStates =
    [
        new("AC", "Acre"),
        new("AL", "Alagoas"),
        new("AP", "Amapá"),
        new("AM", "Amazonas"),
        new("BA", "Bahia"),
        new("CE", "Ceará"),
        new("DF", "Distrito Federal"),
        new("ES", "Espírito Santo"),
        new("GO", "Goiás"),
        new("MA", "Maranhão"),
        new("MT", "Mato Grosso"),
        new("MS", "Mato Grosso do Sul"),
        new("MG", "Minas Gerais"),
        new("PA", "Pará"),
        new("PB", "Paraíba"),
        new("PR", "Paraná"),
        new("PE", "Pernambuco"),
        new("PI", "Piauí"),
        new("RJ", "Rio de Janeiro"),
        new("RN", "Rio Grande do Norte"),
        new("RS", "Rio Grande do Sul"),
        new("RO", "Rondônia"),
        new("RR", "Roraima"),
        new("SC", "Santa Catarina"),
        new("SP", "São Paulo"),
        new("SE", "Sergipe"),
        new("TO", "Tocantins"),
    ];

    public static readonly IReadOnlyList<LookupEntry> RegistrationStatuses =
    [
        new("01", "Nula"),
        new("02", "Ativa"),
        new("03", "Suspensa"),
        new("04", "Inapta"),
        new("08", "Baixada"),
    ];

    public static readonly IReadOnlyList<LookupEntry> CompanySizes =
    [
        new("00", "Não informado"),
        new("01", "Micro empresa"),
        new("03", "Empresa de pequeno porte"),
        new("05", "Demais"),
    ];
}
