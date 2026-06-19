namespace RFBDataProject.Infrastructure.Persistence.Sql;

public static class CnpjSchemaSql
{
    public const string CreateTables = """
        CREATE TABLE IF NOT EXISTS empresas (
            cnpj_basico TEXT,
            razao_social TEXT,
            natureza_juridica TEXT,
            qualificacao_responsavel TEXT,
            capital_social TEXT,
            porte_empresa TEXT,
            ente_federativo_responsavel TEXT
        );

        CREATE TABLE IF NOT EXISTS estabelecimentos (
            cnpj_basico TEXT,
            cnpj_ordem TEXT,
            cnpj_dv TEXT,
            identificador_matriz_filial TEXT,
            nome_fantasia TEXT,
            situacao_cadastral TEXT,
            data_situacao_cadastral TEXT,
            motivo_situacao_cadastral TEXT,
            nome_cidade_exterior TEXT,
            pais TEXT,
            data_inicio_atividade TEXT,
            cnae_fiscal_principal TEXT,
            cnae_fiscal_secundaria TEXT,
            tipo_logradouro TEXT,
            logradouro TEXT,
            numero TEXT,
            complemento TEXT,
            bairro TEXT,
            cep TEXT,
            uf TEXT,
            municipio TEXT,
            ddd_1 TEXT,
            telefone_1 TEXT,
            ddd_2 TEXT,
            telefone_2 TEXT,
            ddd_fax TEXT,
            fax TEXT,
            correio_eletronico TEXT,
            situacao_especial TEXT,
            data_situacao_especial TEXT
        );

        CREATE TABLE IF NOT EXISTS socios (
            cnpj_basico TEXT,
            identificador_socio TEXT,
            nome_socio TEXT,
            cnpj_cpf_socio TEXT,
            qualificacao_socio TEXT,
            data_entrada_sociedade TEXT,
            pais TEXT,
            representante_legal TEXT,
            nome_representante TEXT,
            qualificacao_representante_legal TEXT,
            faixa_etaria TEXT
        );

        CREATE TABLE IF NOT EXISTS simples (
            cnpj_basico TEXT,
            opcao_pelo_simples TEXT,
            data_opcao_simples TEXT,
            data_exclusao_simples TEXT,
            opcao_mei TEXT,
            data_opcao_mei TEXT,
            data_exclusao_mei TEXT
        );

        CREATE TABLE IF NOT EXISTS cnaes (codigo TEXT, descricao TEXT);
        CREATE TABLE IF NOT EXISTS motivos (codigo TEXT, descricao TEXT);
        CREATE TABLE IF NOT EXISTS municipios (codigo TEXT, descricao TEXT);
        CREATE TABLE IF NOT EXISTS naturezas (codigo TEXT, descricao TEXT);
        CREATE TABLE IF NOT EXISTS paises (codigo TEXT, descricao TEXT);
        CREATE TABLE IF NOT EXISTS qualificacoes (codigo TEXT, descricao TEXT);
        """;

    public const string TruncateAll = """
        TRUNCATE TABLE empresas, estabelecimentos, socios, simples,
                      cnaes, motivos, municipios, naturezas, paises, qualificacoes;
        """;

    public const string CreateBtreeIndexes = """
        CREATE INDEX IF NOT EXISTS idx_empresas_cnpj_basico ON empresas (cnpj_basico);
        CREATE INDEX IF NOT EXISTS idx_empresas_natureza_juridica ON empresas (natureza_juridica);
        CREATE INDEX IF NOT EXISTS idx_estabelecimentos_cnpj_basico ON estabelecimentos (cnpj_basico);
        CREATE INDEX IF NOT EXISTS idx_estabelecimentos_uf ON estabelecimentos (uf);
        CREATE INDEX IF NOT EXISTS idx_estabelecimentos_municipio ON estabelecimentos (municipio);
        CREATE INDEX IF NOT EXISTS idx_estabelecimentos_cnae_fiscal_principal ON estabelecimentos (cnae_fiscal_principal);
        CREATE INDEX IF NOT EXISTS idx_socios_cnpj_basico ON socios (cnpj_basico);
        CREATE INDEX IF NOT EXISTS idx_socios_cnpj_cpf_socio ON socios (cnpj_cpf_socio);
        CREATE INDEX IF NOT EXISTS idx_socios_nome_socio ON socios (nome_socio);
        CREATE INDEX IF NOT EXISTS idx_simples_cnpj_basico ON simples (cnpj_basico);
        """;

    public const string CreateSearchIndexes = """
        CREATE EXTENSION IF NOT EXISTS pg_trgm;
        CREATE INDEX IF NOT EXISTS idx_empresas_razao_trgm ON empresas USING gin (razao_social gin_trgm_ops);
        CREATE INDEX IF NOT EXISTS idx_estab_nome_fantasia_trgm ON estabelecimentos USING gin (nome_fantasia gin_trgm_ops);
        """;

    public const string CreateViews = """
        CREATE OR REPLACE VIEW vw_empresas_completo AS
        SELECT
            e.cnpj_basico || est.cnpj_ordem || est.cnpj_dv AS cnpj,
            e.razao_social,
            est.nome_fantasia,
            e.natureza_juridica,
            n.descricao AS natureza_juridica_desc,
            e.capital_social,
            e.porte_empresa,
            est.situacao_cadastral,
            est.data_inicio_atividade,
            est.cnae_fiscal_principal,
            c.descricao AS cnae_principal_desc,
            est.identificador_matriz_filial,
            est.uf,
            est.municipio,
            m.descricao AS municipio_desc,
            est.logradouro,
            est.numero,
            est.bairro,
            est.cep,
            est.correio_eletronico,
            CASE WHEN est.telefone_1 IS NOT NULL AND est.telefone_1 <> ''
                 THEN COALESCE(est.ddd_1, '') || est.telefone_1 ELSE NULL END AS telefone_1
        FROM empresas e
        JOIN estabelecimentos est ON est.cnpj_basico = e.cnpj_basico
        LEFT JOIN naturezas n ON n.codigo = e.natureza_juridica
        LEFT JOIN cnaes c ON c.codigo = est.cnae_fiscal_principal
        LEFT JOIN municipios m ON m.codigo = est.municipio;

        CREATE OR REPLACE VIEW vw_holdings AS
        SELECT DISTINCT
            e.cnpj_basico,
            est.cnpj_basico || est.cnpj_ordem || est.cnpj_dv AS cnpj,
            e.razao_social,
            e.natureza_juridica,
            est.cnae_fiscal_principal,
            est.uf,
            est.municipio
        FROM empresas e
        JOIN estabelecimentos est ON est.cnpj_basico = e.cnpj_basico
        WHERE est.cnae_fiscal_principal IN ('6462000', '6463800')
           OR UPPER(e.razao_social) LIKE '%HOLDING%'
           OR UPPER(e.razao_social) LIKE '%PARTICIPACOES%'
           OR UPPER(e.razao_social) LIKE '%PARTICIPAÇÕES%';

        CREATE OR REPLACE VIEW vw_participacoes_pj AS
        SELECT
            s.cnpj_basico AS cnpj_controlada_basico,
            s.cnpj_cpf_socio AS cnpj_controladora,
            s.nome_socio AS razao_controladora,
            s.qualificacao_socio,
            s.data_entrada_sociedade,
            e.razao_social AS razao_controlada
        FROM socios s
        LEFT JOIN empresas e ON e.cnpj_basico = s.cnpj_basico
        WHERE s.identificador_socio = '1';
        """;
}
