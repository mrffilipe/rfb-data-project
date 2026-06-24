namespace RFBDataProject.Infrastructure.Persistence.Sql;

public static class StagingSchemaSql
{
    public const string CreateTables = """
        CREATE TABLE IF NOT EXISTS receita_empresa_staging (
            execution_id UUID NOT NULL,
            cnpj_basico TEXT,
            razao_social TEXT,
            natureza_juridica TEXT,
            qualificacao_responsavel TEXT,
            capital_social TEXT,
            porte_empresa TEXT,
            ente_federativo_responsavel TEXT,
            row_hash CHAR(32)
        );

        CREATE TABLE IF NOT EXISTS receita_estabelecimento_staging (
            execution_id UUID NOT NULL,
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
            data_situacao_especial TEXT,
            row_hash CHAR(32)
        );

        CREATE TABLE IF NOT EXISTS receita_socio_staging (
            execution_id UUID NOT NULL,
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
            faixa_etaria TEXT,
            row_hash CHAR(32)
        );

        CREATE TABLE IF NOT EXISTS receita_simples_staging (
            execution_id UUID NOT NULL,
            cnpj_basico TEXT,
            opcao_pelo_simples TEXT,
            data_opcao_simples TEXT,
            data_exclusao_simples TEXT,
            opcao_mei TEXT,
            data_opcao_mei TEXT,
            data_exclusao_mei TEXT,
            row_hash CHAR(32)
        );

        CREATE TABLE IF NOT EXISTS receita_cnae_staging (
            execution_id UUID NOT NULL,
            codigo TEXT,
            descricao TEXT,
            row_hash CHAR(32)
        );

        CREATE TABLE IF NOT EXISTS receita_motivo_staging (
            execution_id UUID NOT NULL,
            codigo TEXT,
            descricao TEXT,
            row_hash CHAR(32)
        );

        CREATE TABLE IF NOT EXISTS receita_municipio_staging (
            execution_id UUID NOT NULL,
            codigo TEXT,
            descricao TEXT,
            row_hash CHAR(32)
        );

        CREATE TABLE IF NOT EXISTS receita_natureza_staging (
            execution_id UUID NOT NULL,
            codigo TEXT,
            descricao TEXT,
            row_hash CHAR(32)
        );

        CREATE TABLE IF NOT EXISTS receita_pais_staging (
            execution_id UUID NOT NULL,
            codigo TEXT,
            descricao TEXT,
            row_hash CHAR(32)
        );

        CREATE TABLE IF NOT EXISTS receita_qualificacao_staging (
            execution_id UUID NOT NULL,
            codigo TEXT,
            descricao TEXT,
            row_hash CHAR(32)
        );
        """;

    public const string CreateIndexes = """
        CREATE INDEX IF NOT EXISTS idx_stg_empresa_exec ON receita_empresa_staging (execution_id, cnpj_basico);
        CREATE INDEX IF NOT EXISTS idx_stg_estab_exec ON receita_estabelecimento_staging (execution_id, cnpj_basico, cnpj_ordem, cnpj_dv);
        CREATE INDEX IF NOT EXISTS idx_stg_socio_exec ON receita_socio_staging (execution_id, cnpj_basico, identificador_socio, nome_socio, cnpj_cpf_socio);
        CREATE INDEX IF NOT EXISTS idx_stg_simples_exec ON receita_simples_staging (execution_id, cnpj_basico);
        CREATE INDEX IF NOT EXISTS idx_stg_cnae_exec ON receita_cnae_staging (execution_id, codigo);
        CREATE INDEX IF NOT EXISTS idx_stg_motivo_exec ON receita_motivo_staging (execution_id, codigo);
        CREATE INDEX IF NOT EXISTS idx_stg_municipio_exec ON receita_municipio_staging (execution_id, codigo);
        CREATE INDEX IF NOT EXISTS idx_stg_natureza_exec ON receita_natureza_staging (execution_id, codigo);
        CREATE INDEX IF NOT EXISTS idx_stg_pais_exec ON receita_pais_staging (execution_id, codigo);
        CREATE INDEX IF NOT EXISTS idx_stg_qualificacao_exec ON receita_qualificacao_staging (execution_id, codigo);
        """;

    public const string CreateFilterIndexes = """
        CREATE INDEX IF NOT EXISTS idx_stg_estab_exec_uf ON receita_estabelecimento_staging (execution_id, uf);
        CREATE INDEX IF NOT EXISTS idx_stg_estab_exec_cnae ON receita_estabelecimento_staging (execution_id, cnae_fiscal_principal);
        CREATE INDEX IF NOT EXISTS idx_stg_estab_exec_situacao ON receita_estabelecimento_staging (execution_id, situacao_cadastral);
        CREATE INDEX IF NOT EXISTS idx_stg_estab_exec_matriz ON receita_estabelecimento_staging (execution_id, identificador_matriz_filial);
        CREATE INDEX IF NOT EXISTS idx_stg_empresa_exec_natureza ON receita_empresa_staging (execution_id, natureza_juridica);
        CREATE INDEX IF NOT EXISTS idx_stg_empresa_exec_porte ON receita_empresa_staging (execution_id, porte_empresa);
        CREATE INDEX IF NOT EXISTS idx_stg_estab_exec_uf_sort
            ON receita_estabelecimento_staging (execution_id, uf, cnpj_basico, cnpj_ordem);
        """;

    public const string CreateSearchIndexes = """
        CREATE EXTENSION IF NOT EXISTS pg_trgm;
        CREATE INDEX IF NOT EXISTS idx_stg_empresa_razao_trgm
            ON receita_empresa_staging USING gin (razao_social gin_trgm_ops);
        CREATE INDEX IF NOT EXISTS idx_stg_estab_nome_trgm
            ON receita_estabelecimento_staging USING gin (nome_fantasia gin_trgm_ops);
        """;

    public const string DropDomainTables = """
        DROP VIEW IF EXISTS vw_empresas_completo CASCADE;
        DROP VIEW IF EXISTS vw_holdings CASCADE;
        DROP VIEW IF EXISTS vw_participacoes_pj CASCADE;
        DROP TABLE IF EXISTS empresas CASCADE;
        DROP TABLE IF EXISTS estabelecimentos CASCADE;
        DROP TABLE IF EXISTS socios CASCADE;
        DROP TABLE IF EXISTS simples CASCADE;
        DROP TABLE IF EXISTS companies CASCADE;
        DROP TABLE IF EXISTS establishments CASCADE;
        DROP TABLE IF EXISTS partners CASCADE;
        DROP TABLE IF EXISTS simples_regimes CASCADE;
        DROP TABLE IF EXISTS cnaes CASCADE;
        DROP TABLE IF EXISTS reasons CASCADE;
        DROP TABLE IF EXISTS municipalities CASCADE;
        DROP TABLE IF EXISTS legal_natures CASCADE;
        DROP TABLE IF EXISTS countries CASCADE;
        DROP TABLE IF EXISTS qualifications CASCADE;
        DROP TABLE IF EXISTS motivos CASCADE;
        DROP TABLE IF EXISTS municipios CASCADE;
        DROP TABLE IF EXISTS naturezas CASCADE;
        DROP TABLE IF EXISTS paises CASCADE;
        DROP TABLE IF EXISTS qualificacoes CASCADE;
        """;

    public static string ClearExecution(string stagingTable) =>
        $"DELETE FROM {stagingTable} WHERE execution_id = @execution_id";
}
