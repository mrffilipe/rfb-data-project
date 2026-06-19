import SearchIcon from '@mui/icons-material/Search'
import { Button, Stack, TableCell, TableRow, TextField } from '@mui/material'
import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router'
import {
  DataTable,
  FeedbackAlerts,
  FormActions,
  FormGrid,
  FormGridItem,
  PageHeader,
  SectionCard,
  TablePaginationBar,
} from '../components/ui'
import { listCorporateParticipations } from '../services'
import type { CorporateParticipation } from '../types'
import { getApiErrorMessage } from '../utils/apiError'
import { formatCnpj } from '../utils/formatters'

const PAGE_SIZE = 20

export function CorporateParticipationsPage() {
  const [searchParams] = useSearchParams()
  const [controllingCnpj, setControllingCnpj] = useState(searchParams.get('controllingCnpj') ?? '')
  const [controlledCnpj, setControlledCnpj] = useState(searchParams.get('controlledCnpj') ?? '')

  const [items, setItems] = useState<CorporateParticipation[]>([])
  const [page, setPage] = useState(1)
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searched, setSearched] = useState(false)

  async function runSearch(targetPage: number, overrides?: { controlling?: string; controlled?: string }): Promise<void> {
    setLoading(true)
    setError(null)
    try {
      const result = await listCorporateParticipations({
        controllingCnpj: (overrides?.controlling ?? controllingCnpj).trim() || undefined,
        controlledCnpj: (overrides?.controlled ?? controlledCnpj).trim() || undefined,
        page: targetPage,
        pageSize: PAGE_SIZE,
      })
      setItems(result.items)
      setTotal(result.total)
      setPage(result.page)
      setSearched(true)
    } catch (searchError) {
      setError(getApiErrorMessage(searchError))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    const controlling = searchParams.get('controllingCnpj')
    if (controlling) {
      setControllingCnpj(controlling)
      void runSearch(1, { controlling })
    }
  }, [searchParams])

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    await runSearch(1)
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Participações societárias"
        description="Relações de controle entre empresas (CNPJ controlador e controlado)."
      />
      <FeedbackAlerts error={error} />

      <SectionCard title="Filtros">
        <Stack component="form" onSubmit={handleSubmit} spacing={2}>
          <FormGrid>
            <FormGridItem>
              <TextField
                label="CNPJ controlador"
                value={controllingCnpj}
                onChange={(e) => setControllingCnpj(e.target.value)}
                fullWidth
              />
            </FormGridItem>
            <FormGridItem>
              <TextField
                label="CNPJ controlado"
                value={controlledCnpj}
                onChange={(e) => setControlledCnpj(e.target.value)}
                fullWidth
              />
            </FormGridItem>
          </FormGrid>
          <FormActions>
            <Button type="submit" variant="contained" startIcon={<SearchIcon />} disabled={loading}>
              Buscar
            </Button>
          </FormActions>
        </Stack>
      </SectionCard>

      {searched ? (
        <SectionCard title="Resultados">
          <DataTable
            columns={[
              { id: 'controlling', label: 'CNPJ controlador', minWidth: 150 },
              { id: 'controllingName', label: 'Razão social controlador', minWidth: 180 },
              { id: 'controlled', label: 'CNPJ base controlado', minWidth: 150 },
              { id: 'controlledName', label: 'Razão social controlado', minWidth: 180 },
              { id: 'qualification', label: 'Qualificação' },
              { id: 'entryDate', label: 'Entrada' },
            ]}
            loading={loading}
            rows={items.map((item, index) => (
              <TableRow key={`${item.controllingCnpj}-${item.controlledCnpjBase}-${index}`} hover>
                <TableCell>{formatCnpj(item.controllingCnpj)}</TableCell>
                <TableCell>{item.controllingLegalName ?? '—'}</TableCell>
                <TableCell>{item.controlledCnpjBase}</TableCell>
                <TableCell>{item.controlledLegalName ?? '—'}</TableCell>
                <TableCell>{item.partnerQualificationCode ?? '—'}</TableCell>
                <TableCell>{item.entryDate ?? '—'}</TableCell>
              </TableRow>
            ))}
            emptyTitle="Nenhuma participação encontrada"
          />
          <TablePaginationBar
            page={page}
            pageSize={PAGE_SIZE}
            total={total}
            onPageChange={(nextPage) => void runSearch(nextPage)}
            disabled={loading}
          />
        </SectionCard>
      ) : null}
    </Stack>
  )
}
