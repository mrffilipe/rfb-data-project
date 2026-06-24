namespace RFBDataProject.Infrastructure.Persistence.Sql;

public static class StagingQuerySql
{
    public const string CompanyCompleteFrom = """
        FROM receita_empresa_staging c
        INNER JOIN receita_estabelecimento_staging e
            ON e.cnpj_basico = c.cnpj_basico AND e.execution_id = c.execution_id
        LEFT JOIN receita_natureza_staging ln
            ON ln.codigo = c.natureza_juridica AND ln.execution_id = c.execution_id
        LEFT JOIN receita_cnae_staging cn
            ON cn.codigo = e.cnae_fiscal_principal AND cn.execution_id = c.execution_id
        LEFT JOIN receita_municipio_staging m
            ON m.codigo = e.municipio AND m.execution_id = c.execution_id
        """;

    public const string CompanyCompleteSelect = """
        SELECT c.cnpj_basico || e.cnpj_ordem || e.cnpj_dv AS cnpj,
               c.razao_social AS razao_social,
               e.nome_fantasia AS nome_fantasia,
               c.natureza_juridica AS natureza_juridica,
               ln.descricao AS natureza_juridica_desc,
               c.capital_social AS capital_social,
               c.porte_empresa AS porte_empresa,
               e.situacao_cadastral AS situacao_cadastral,
               e.data_inicio_atividade AS data_inicio_atividade,
               e.cnae_fiscal_principal AS cnae_fiscal_principal,
               cn.descricao AS cnae_principal_desc,
               e.identificador_matriz_filial AS identificador_matriz_filial,
               e.uf AS uf,
               e.municipio AS municipio,
               m.descricao AS municipio_desc,
               e.logradouro AS logradouro,
               e.numero AS numero,
               e.bairro AS bairro,
               e.cep AS cep,
               e.correio_eletronico AS correio_eletronico,
               CASE WHEN e.telefone_1 IS NOT NULL AND e.telefone_1 <> ''
                    THEN COALESCE(e.ddd_1, '') || e.telefone_1 ELSE NULL END AS telefone_1
        """;

    public const string CompanySummarySelect = """
        SELECT c.cnpj_basico || e.cnpj_ordem || e.cnpj_dv AS cnpj,
               c.razao_social AS razao_social,
               e.nome_fantasia AS nome_fantasia,
               e.uf AS uf,
               m.descricao AS municipio_desc,
               e.cnae_fiscal_principal AS cnae_fiscal_principal,
               cn.descricao AS cnae_principal_desc,
               e.situacao_cadastral AS situacao_cadastral
        """;

    public const string HoldingsFrom = """
        FROM receita_empresa_staging c
        INNER JOIN receita_estabelecimento_staging e
            ON e.cnpj_basico = c.cnpj_basico AND e.execution_id = c.execution_id
        WHERE c.execution_id = @execution_id
          AND (e.cnae_fiscal_principal IN ('6462000', '6463800')
           OR UPPER(c.razao_social) LIKE '%HOLDING%'
           OR UPPER(c.razao_social) LIKE '%PARTICIPACOES%'
           OR UPPER(c.razao_social) LIKE '%PARTICIPAÇÕES%')
        """;

    public const string HoldingsSelect = """
        SELECT DISTINCT c.cnpj_basico,
               c.cnpj_basico || e.cnpj_ordem || e.cnpj_dv AS cnpj,
               c.razao_social AS razao_social,
               c.natureza_juridica AS natureza_juridica,
               e.cnae_fiscal_principal AS cnae_fiscal_principal,
               e.uf AS uf,
               e.municipio AS municipio
        """;

    public const string CorporateParticipationsFrom = """
        FROM receita_socio_staging p
        LEFT JOIN receita_empresa_staging c
            ON c.cnpj_basico = p.cnpj_basico AND c.execution_id = p.execution_id
        WHERE p.identificador_socio = '1'
          AND p.execution_id = @execution_id
        """;

    public const string CorporateParticipationsSelect = """
        SELECT p.cnpj_basico AS cnpj_controlada_basico,
               p.cnpj_cpf_socio AS cnpj_controladora,
               p.nome_socio AS razao_controladora,
               p.qualificacao_socio AS qualificacao_socio,
               p.data_entrada_sociedade AS data_entrada_sociedade,
               c.razao_social AS razao_controlada
        """;
}
