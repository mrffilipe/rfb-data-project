import SyncIcon from '@mui/icons-material/Sync'
import { Button, Chip, Stack, TableCell, TableRow } from '@mui/material'
import { useCallback, useEffect, useState } from 'react'
import {
  ConfirmDialog,
  DataTable,
  FeedbackAlerts,
  FormGrid,
  FormGridItem,
  PageHeader,
  SectionCard,
  StaticField,
} from '../components/ui'
import { getIngestionStatus, triggerIngestionSync } from '../services'
import type { IngestionStatus } from '../types'
import { getApiErrorMessage } from '../utils/apiError'
import { formatDateTime } from '../utils/formatters'

const POLL_INTERVAL_MS = 5000

export function IngestionPage() {
  const [status, setStatus] = useState<IngestionStatus | null>(null)
  const [loading, setLoading] = useState(true)
  const [syncing, setSyncing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [confirmOpen, setConfirmOpen] = useState(false)

  const loadStatus = useCallback(async (): Promise<void> => {
    try {
      const data = await getIngestionStatus()
      setStatus(data)
      setError(null)
    } catch (loadError) {
      setError(getApiErrorMessage(loadError))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadStatus()
  }, [loadStatus])

  useEffect(() => {
    if (!status?.isSyncRunning) {
      return
    }

    const interval = window.setInterval(() => {
      void loadStatus()
    }, POLL_INTERVAL_MS)

    return () => window.clearInterval(interval)
  }, [status?.isSyncRunning, loadStatus])

  async function handleSync(): Promise<void> {
    setSyncing(true)
    setError(null)
    setSuccess(null)
    try {
      const data = await triggerIngestionSync()
      setStatus(data)
      setSuccess('Sincronização iniciada com sucesso.')
      setConfirmOpen(false)
    } catch (syncError) {
      setError(getApiErrorMessage(syncError))
    } finally {
      setSyncing(false)
    }
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Ingestão de dados"
        description="Monitoramento da carga e sincronização dos dados públicos da Receita Federal."
        actions={
          <Button
            variant="contained"
            startIcon={<SyncIcon />}
            onClick={() => setConfirmOpen(true)}
            disabled={loading || syncing || status?.isSyncRunning}
          >
            Sincronizar agora
          </Button>
        }
      />
      <FeedbackAlerts error={error} success={success} onDismissSuccess={() => setSuccess(null)} />

      <SectionCard title="Status geral">
        {status ? (
          <>
            <Stack direction="row" spacing={1} sx={{ mb: 2, flexWrap: 'wrap' }}>
              <Chip
                size="small"
                label={status.isDataReady ? 'Dados prontos' : 'Dados em preparação'}
                color={status.isDataReady ? 'success' : 'warning'}
                variant="outlined"
              />
              {status.isSyncRunning ? (
                <Chip size="small" label="Sincronização em andamento" color="info" variant="outlined" />
              ) : null}
              {status.releaseStatus ? (
                <Chip size="small" label={`Release: ${status.releaseStatus}`} variant="outlined" />
              ) : null}
            </Stack>
            <FormGrid>
              <FormGridItem>
                <StaticField label="Competência ativa" value={status.activeReferencePeriod} />
              </FormGridItem>
              <FormGridItem>
                <StaticField label="Última competência" value={status.latestReferencePeriod} />
              </FormGridItem>
              <FormGridItem>
                <StaticField
                  label="Artefatos"
                  value={`${status.loadedArtifacts} carregados / ${status.totalArtifacts} total (${status.failedArtifacts} falhas)`}
                />
              </FormGridItem>
              <FormGridItem>
                <StaticField label="Início da última sync" value={formatDateTime(status.lastSyncStartedAt)} />
              </FormGridItem>
              <FormGridItem>
                <StaticField label="Fim da última sync" value={formatDateTime(status.lastSyncCompletedAt)} />
              </FormGridItem>
              <FormGridItem md={12}>
                <StaticField label="Último erro" value={status.lastError} />
              </FormGridItem>
            </FormGrid>
          </>
        ) : null}
      </SectionCard>

      <SectionCard title="Artefatos">
        <DataTable
          columns={[
            { id: 'fileName', label: 'Arquivo', minWidth: 200 },
            { id: 'targetTable', label: 'Tabela destino', minWidth: 140 },
            { id: 'status', label: 'Status' },
            { id: 'size', label: 'Tamanho (bytes)', align: 'right' as const },
            { id: 'loadedAt', label: 'Carregado em', minWidth: 160 },
          ]}
          loading={loading && !status}
          rows={(status?.artifacts ?? []).map((artifact) => (
            <TableRow key={`${artifact.fileName}-${artifact.targetTable}`} hover>
              <TableCell>{artifact.fileName}</TableCell>
              <TableCell>{artifact.targetTable}</TableCell>
              <TableCell>{artifact.status}</TableCell>
              <TableCell align="right">{artifact.remoteSize?.toLocaleString('pt-BR') ?? '—'}</TableCell>
              <TableCell>{formatDateTime(artifact.loadedAt)}</TableCell>
            </TableRow>
          ))}
          emptyTitle="Nenhum artefato registrado"
        />
      </SectionCard>

      <ConfirmDialog
        open={confirmOpen}
        onClose={() => setConfirmOpen(false)}
        onConfirm={handleSync}
        title="Iniciar sincronização"
        message="Deseja iniciar uma nova sincronização com a Receita Federal? O processo pode levar vários minutos."
        confirmLabel="Sincronizar"
        loading={syncing}
      />
    </Stack>
  )
}
