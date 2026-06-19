import AccountTreeOutlinedIcon from '@mui/icons-material/AccountTreeOutlined'
import BusinessOutlinedIcon from '@mui/icons-material/BusinessOutlined'
import ChevronRightIcon from '@mui/icons-material/ChevronRight'
import CloudSyncOutlinedIcon from '@mui/icons-material/CloudSyncOutlined'
import GroupsOutlinedIcon from '@mui/icons-material/GroupsOutlined'
import SearchOutlinedIcon from '@mui/icons-material/SearchOutlined'
import ViewListOutlinedIcon from '@mui/icons-material/ViewListOutlined'
import { Box, Card, CardActionArea, CardContent, Chip, Grid, Stack, Typography } from '@mui/material'
import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { FeedbackAlerts, PageHeader, SectionCard, StatusChip } from '../components/ui'
import { checkHealth, getIngestionStatus } from '../services'
import type { IngestionStatus } from '../types'
import { getApiErrorMessage } from '../utils/apiError'
import { formatDateTime } from '../utils/formatters'

const modules = [
  {
    to: '/companies/search',
    label: 'Busca de empresas',
    description: 'Pesquisa avançada por texto, UF, CNAE e mais',
    icon: <SearchOutlinedIcon />,
  },
  {
    to: '/companies',
    label: 'Listagem',
    description: 'Empresas filtradas por UF, CNAE e natureza jurídica',
    icon: <ViewListOutlinedIcon />,
  },
  {
    to: '/companies/holdings',
    label: 'Holdings',
    description: 'Empresas controladoras e participações',
    icon: <BusinessOutlinedIcon />,
  },
  {
    to: '/partners',
    label: 'Sócios',
    description: 'Consulta por documento ou CNPJ da empresa',
    icon: <GroupsOutlinedIcon />,
  },
  {
    to: '/participations/corporate',
    label: 'Participações societárias',
    description: 'Relações de controle entre empresas',
    icon: <AccountTreeOutlinedIcon />,
  },
  {
    to: '/ingestion',
    label: 'Ingestão',
    description: 'Status da carga de dados da Receita Federal',
    icon: <CloudSyncOutlinedIcon />,
  },
]

export function HomePage() {
  const [ingestion, setIngestion] = useState<IngestionStatus | null>(null)
  const [apiHealthy, setApiHealthy] = useState<boolean | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function load(): Promise<void> {
      try {
        const [health, status] = await Promise.all([checkHealth(), getIngestionStatus()])
        setApiHealthy(health.status === 'ok')
        setIngestion(status)
      } catch (loadError) {
        setError(getApiErrorMessage(loadError))
        setApiHealthy(false)
      }
    }

    void load()
  }, [])

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Dashboard"
        description="Consulta e exploração de dados públicos de CNPJ da Receita Federal do Brasil."
        actions={
          apiHealthy === null ? null : (
            <StatusChip
              label={apiHealthy ? 'API online' : 'API indisponível'}
              variant={apiHealthy ? 'success' : 'error'}
            />
          )
        }
      />
      <FeedbackAlerts error={error} />

      {ingestion ? (
        <SectionCard title="Status da ingestão" subtitle="Resumo da competência ativa e sincronização">
          <Stack direction="row" spacing={1} sx={{ mb: 2, flexWrap: 'wrap' }}>
            <Chip
              size="small"
              label={ingestion.isDataReady ? 'Dados prontos' : 'Dados em preparação'}
              color={ingestion.isDataReady ? 'success' : 'warning'}
              variant="outlined"
            />
            {ingestion.isSyncRunning ? (
              <Chip size="small" label="Sincronização em andamento" color="info" variant="outlined" />
            ) : null}
            {ingestion.activeReferencePeriod ? (
              <Chip size="small" label={`Competência: ${ingestion.activeReferencePeriod}`} variant="outlined" />
            ) : null}
          </Stack>
          <Typography variant="body2" color="text.secondary">
            {ingestion.loadedArtifacts} de {ingestion.totalArtifacts} artefatos carregados
            {ingestion.lastSyncCompletedAt ? ` · Última sync: ${formatDateTime(ingestion.lastSyncCompletedAt)}` : null}
          </Typography>
        </SectionCard>
      ) : null}

      <Grid container spacing={2}>
        {modules.map((module) => (
          <Grid key={module.to} size={{ xs: 12, sm: 6, lg: 4 }}>
            <Card sx={{ height: '100%' }}>
              <CardActionArea component={Link} to={module.to} sx={{ height: '100%' }}>
                <CardContent>
                  <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
                    <Box
                      sx={{
                        p: 1,
                        borderRadius: 2,
                        bgcolor: 'primary.main',
                        color: 'primary.contrastText',
                        display: 'flex',
                        flexShrink: 0,
                      }}
                    >
                      {module.icon}
                    </Box>
                    <Stack spacing={0.5} sx={{ flex: 1, minWidth: 0 }}>
                      <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                        {module.label}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {module.description}
                      </Typography>
                    </Stack>
                    <ChevronRightIcon fontSize="small" sx={{ color: 'text.secondary', flexShrink: 0 }} />
                  </Stack>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Stack>
  )
}
