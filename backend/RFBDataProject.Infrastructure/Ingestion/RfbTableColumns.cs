using RFBDataProject.Domain.Constants;

namespace RFBDataProject.Infrastructure.Ingestion;

public static class RfbTableColumns
{
    private static readonly IReadOnlyDictionary<string, string[]> Columns = new Dictionary<string, string[]>
    {
        [RfbTableNames.Companies] =
        [
            "cnpj_basico", "razao_social", "natureza_juridica", "qualificacao_responsavel",
            "capital_social", "porte_empresa", "ente_federativo_responsavel"
        ],
        [RfbTableNames.Establishments] =
        [
            "cnpj_basico", "cnpj_ordem", "cnpj_dv", "identificador_matriz_filial", "nome_fantasia",
            "situacao_cadastral", "data_situacao_cadastral", "motivo_situacao_cadastral",
            "nome_cidade_exterior", "pais", "data_inicio_atividade", "cnae_fiscal_principal",
            "cnae_fiscal_secundaria", "tipo_logradouro", "logradouro", "numero", "complemento",
            "bairro", "cep", "uf", "municipio", "ddd_1", "telefone_1", "ddd_2", "telefone_2",
            "ddd_fax", "fax", "correio_eletronico", "situacao_especial", "data_situacao_especial"
        ],
        [RfbTableNames.Partners] =
        [
            "cnpj_basico", "identificador_socio", "nome_socio", "cnpj_cpf_socio",
            "qualificacao_socio", "data_entrada_sociedade", "pais", "representante_legal",
            "nome_representante", "qualificacao_representante_legal", "faixa_etaria"
        ],
        [RfbTableNames.SimplesNational] =
        [
            "cnpj_basico", "opcao_pelo_simples", "data_opcao_simples", "data_exclusao_simples",
            "opcao_mei", "data_opcao_mei", "data_exclusao_mei"
        ],
        [RfbTableNames.Cnaes] = ["codigo", "descricao"],
        [RfbTableNames.Reasons] = ["codigo", "descricao"],
        [RfbTableNames.Municipalities] = ["codigo", "descricao"],
        [RfbTableNames.LegalNatures] = ["codigo", "descricao"],
        [RfbTableNames.Countries] = ["codigo", "descricao"],
        [RfbTableNames.Qualifications] = ["codigo", "descricao"]
    };

    public static string[] GetColumns(string tableName) =>
        Columns.TryGetValue(tableName, out var cols)
            ? cols
            : throw new ArgumentException($"Unknown table: {tableName}", nameof(tableName));
}
