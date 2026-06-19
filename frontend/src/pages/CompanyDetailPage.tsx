import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import { Button, Grid, Stack } from '@mui/material'
import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { FeedbackAlerts, FormGrid, FormGridItem, PageHeader, SectionCard, StaticField } from '../components/ui'
import { getCompanyByCnpj } from '../services'
import type { CompanyDetail } from '../types'
import { getApiErrorMessage } from '../utils/apiError'
import { formatCnpj } from '../utils/formatters'

export function CompanyDetailPage() {
  const { cnpj = '' } = useParams()
  const navigate = useNavigate()
  const [company, setCompany] = useState<CompanyDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function load(): Promise<void> {
      if (!cnpj) {
        return
      }
      setLoading(true)
      setError(null)
      try {
        const data = await getCompanyByCnpj(cnpj)
        setCompany(data)
      } catch (loadError) {
        setError(getApiErrorMessage(loadError))
        setCompany(null)
      } finally {
        setLoading(false)
      }
    }

    void load()
  }, [cnpj])

  const displayCnpj = company?.cnpj ?? cnpj

  return (
    <Stack spacing={3}>
      <PageHeader
        title={loading ? 'Carregando…' : company?.legalName ?? 'Empresa'}
        description={company ? formatCnpj(company.cnpj) : formatCnpj(cnpj)}
        actions={
          <Stack direction="row" spacing={1}>
            <Button component={Link} to={`/partners?cnpj=${displayCnpj}`} variant="outlined" size="small">
              Ver sócios
            </Button>
            <Button
              component={Link}
              to={`/participations/corporate?controllingCnpj=${displayCnpj}`}
              variant="outlined"
              size="small"
            >
              Participações
            </Button>
          </Stack>
        }
      />
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => navigate(-1)}
        sx={{ alignSelf: 'flex-start', color: 'text.secondary' }}
      >
        Voltar
      </Button>
      <FeedbackAlerts error={error} />

      {company ? (
        <>
          <SectionCard title="Dados cadastrais">
            <FormGrid>
              <FormGridItem>
                <StaticField label="CNPJ" value={formatCnpj(company.cnpj)} monospace />
              </FormGridItem>
              <FormGridItem>
                <StaticField label="Razão social" value={company.legalName} />
              </FormGridItem>
              <FormGridItem>
                <StaticField label="Nome fantasia" value={company.tradeName} />
              </FormGridItem>
              <FormGridItem>
                <StaticField label="Situação cadastral" value={company.registrationStatus} />
              </FormGridItem>
              <FormGridItem>
                <StaticField label="Data início atividade" value={company.activityStartDate} />
              </FormGridItem>
              <FormGridItem>
                <StaticField label="Capital social" value={company.shareCapital} />
              </FormGridItem>
              <FormGridItem>
                <StaticField label="Porte" value={company.companySizeCode} />
              </FormGridItem>
              <FormGridItem>
                <StaticField
                  label="Natureza jurídica"
                  value={
                    company.legalNatureCode
                      ? `${company.legalNatureCode}${company.legalNatureDescription ? ` — ${company.legalNatureDescription}` : ''}`
                      : null
                  }
                />
              </FormGridItem>
              <FormGridItem md={12}>
                <StaticField
                  label="CNAE principal"
                  value={
                    company.primaryCnaeCode
                      ? `${company.primaryCnaeCode}${company.primaryCnaeDescription ? ` — ${company.primaryCnaeDescription}` : ''}`
                      : null
                  }
                />
              </FormGridItem>
            </FormGrid>
          </SectionCard>

          <SectionCard title="Endereço e contato">
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, md: 6 }}>
                <StaticField label="Logradouro" value={company.streetName} />
              </Grid>
              <Grid size={{ xs: 12, md: 3 }}>
                <StaticField label="Número" value={company.streetNumber} />
              </Grid>
              <Grid size={{ xs: 12, md: 3 }}>
                <StaticField label="Bairro" value={company.neighborhood} />
              </Grid>
              <Grid size={{ xs: 12, md: 4 }}>
                <StaticField label="CEP" value={company.zipCode} />
              </Grid>
              <Grid size={{ xs: 12, md: 4 }}>
                <StaticField label="UF" value={company.stateCode} />
              </Grid>
              <Grid size={{ xs: 12, md: 4 }}>
                <StaticField
                  label="Município"
                  value={
                    company.municipalityDescription ??
                    (company.municipalityCode ? `Código ${company.municipalityCode}` : null)
                  }
                />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <StaticField label="E-mail" value={company.email} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <StaticField label="Telefone" value={company.phoneNumber} />
              </Grid>
            </Grid>
          </SectionCard>
        </>
      ) : null}
    </Stack>
  )
}
