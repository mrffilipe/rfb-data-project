import DataObjectIcon from '@mui/icons-material/DataObject'
import GridOnIcon from '@mui/icons-material/GridOn'
import TableViewIcon from '@mui/icons-material/TableView'
import { Button, CircularProgress, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import type { CompanyExportResult } from '../../types'
import { getApiErrorMessage, isApiTimeoutError } from '../../utils/apiError'
import {
  downloadExportCsv,
  downloadExportJson,
  downloadExportXlsx,
  formatExportStats,
} from '../../utils/exportFiles'
import { CheckboxField } from './CheckboxField'
import { FeedbackAlerts } from './FeedbackAlerts'
import { FormGrid, FormGridItem } from './FormGrid'

type ExportFormat = 'xlsx' | 'csv' | 'json'

const EXPORT_TIMEOUT_MESSAGE =
  'A exportação demorou mais que o esperado. Tente um limite menor ou aguarde — o servidor pode ainda estar processando.'

interface CompanyExportPanelProps<TParams> {
  buildExportParams: () => TParams
  exportFn: (params: TParams & { limit: number; deduplicateEmail: boolean }) => Promise<CompanyExportResult>
  disabled?: boolean
}

export function CompanyExportPanel<TParams>({
  buildExportParams,
  exportFn,
  disabled = false,
}: CompanyExportPanelProps<TParams>) {
  const [limit, setLimit] = useState('100')
  const [deduplicateEmail, setDeduplicateEmail] = useState(false)
  const [loading, setLoading] = useState(false)
  const [exportingFormat, setExportingFormat] = useState<ExportFormat | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [lastExport, setLastExport] = useState<CompanyExportResult | null>(null)

  async function handleExport(format: ExportFormat): Promise<void> {
    const parsedLimit = Number(limit)
    if (!Number.isFinite(parsedLimit) || parsedLimit < 1 || parsedLimit > 10_000) {
      setError('A quantidade deve ser entre 1 e 10.000.')
      return
    }

    setLoading(true)
    setExportingFormat(format)
    setError(null)
    try {
      const result = await exportFn({
        ...buildExportParams(),
        limit: parsedLimit,
        deduplicateEmail,
      })
      setLastExport(result)

      if (format === 'json') downloadExportJson(result)
      if (format === 'csv') downloadExportCsv(result)
      if (format === 'xlsx') downloadExportXlsx(result)
    } catch (exportError) {
      setError(isApiTimeoutError(exportError) ? EXPORT_TIMEOUT_MESSAGE : getApiErrorMessage(exportError))
    } finally {
      setLoading(false)
      setExportingFormat(null)
    }
  }

  function buttonLabel(format: ExportFormat, defaultLabel: string): string {
    return loading && exportingFormat === format ? 'Exportando…' : defaultLabel
  }

  return (
    <Stack spacing={1.5}>
      <FeedbackAlerts error={error} />
      <FormGrid>
        <FormGridItem md={4}>
          <TextField
            label="Quantidade"
            type="number"
            value={limit}
            onChange={(event) => setLimit(event.target.value)}
            slotProps={{ htmlInput: { min: 1, max: 10_000 } }}
            helperText="Leads com e-mail (padrão: 100)."
            fullWidth
            disabled={disabled || loading}
          />
        </FormGridItem>
        <FormGridItem md={8}>
          <CheckboxField
            label="Ignorar e-mails duplicados"
            checked={deduplicateEmail}
            onCheckedChange={setDeduplicateEmail}
            disabled={disabled || loading}
          />
          <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, display: 'block' }}>
            Apenas empresas com e-mail são exportadas. A quantidade refere-se a leads com e-mail.
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
            Com deduplicação ativa, e-mails repetidos são ignorados até atingir a quantidade
            solicitada.
          </Typography>
        </FormGridItem>
      </FormGrid>

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
        <Button
          variant="outlined"
          startIcon={<GridOnIcon />}
          onClick={() => void handleExport('xlsx')}
          disabled={disabled || loading}
          loading={loading && exportingFormat === 'xlsx'}
        >
          {buttonLabel('xlsx', 'Exportar XLSX')}
        </Button>
        <Button
          variant="outlined"
          startIcon={<TableViewIcon />}
          onClick={() => void handleExport('csv')}
          disabled={disabled || loading}
          loading={loading && exportingFormat === 'csv'}
        >
          {buttonLabel('csv', 'Exportar CSV')}
        </Button>
        <Button
          variant="outlined"
          startIcon={<DataObjectIcon />}
          onClick={() => void handleExport('json')}
          disabled={disabled || loading}
          loading={loading && exportingFormat === 'json'}
        >
          {buttonLabel('json', 'Exportar JSON')}
        </Button>
      </Stack>

      {loading ? (
        <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center' }}>
          <CircularProgress size={20} />
          <Typography variant="body2" color="text.secondary">
            Exportando… isso pode levar alguns minutos para volumes grandes.
          </Typography>
        </Stack>
      ) : null}

      {lastExport ? (
        <Typography variant="body2" color="text.secondary">
          {formatExportStats(lastExport.stats)}
        </Typography>
      ) : null}
    </Stack>
  )
}
