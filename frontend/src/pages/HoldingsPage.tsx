import SearchIcon from '@mui/icons-material/Search'
import { Button, Link as MuiLink, Stack, TableCell, TableRow } from '@mui/material'
import { useCallback, useState } from 'react'
import { Link } from 'react-router'
import {
  DataTable,
  FeedbackAlerts,
  FormActions,
  FormGrid,
  FormGridItem,
  LookupAutocomplete,
  PageHeader,
  SectionCard,
  TablePaginationBar,
} from '../components/ui'
import { listHoldings, listStates } from '../services'
import type { CompanySummary, LookupItem } from '../types'
import { getApiErrorMessage } from '../utils/apiError'
import { formatCnpj } from '../utils/formatters'

const PAGE_SIZE = 20

const columns = [
  { id: 'cnpj', label: 'CNPJ', minWidth: 140 },
  { id: 'legalName', label: 'Razão social', minWidth: 200 },
  { id: 'tradeName', label: 'Fantasia', minWidth: 160 },
  { id: 'stateCode', label: 'UF' },
  { id: 'municipality', label: 'Município', minWidth: 140 },
]

export function HoldingsPage() {
  const [state, setState] = useState<LookupItem | null>(null)
  const [items, setItems] = useState<CompanySummary[]>([])
  const [page, setPage] = useState(1)
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searched, setSearched] = useState(false)

  const loadStates = useCallback(() => listStates(), [])

  async function runSearch(targetPage: number): Promise<void> {
    setLoading(true)
    setError(null)
    try {
      const result = await listHoldings({
        stateCode: state?.code,
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

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    await runSearch(1)
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Holdings"
        description="Empresas com participações societárias em outras companhias."
      />
      <FeedbackAlerts error={error} />

      <SectionCard title="Filtros">
        <Stack component="form" onSubmit={handleSubmit} spacing={2}>
          <FormGrid>
            <FormGridItem md={4}>
              <LookupAutocomplete
                label="UF"
                placeholder="Buscar estado"
                value={state}
                onChange={setState}
                loadOptions={loadStates}
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
            columns={columns}
            loading={loading}
            rows={items.map((company) => (
              <TableRow key={company.cnpj} hover>
                <TableCell>
                  <MuiLink component={Link} to={`/companies/${company.cnpj}`} underline="hover">
                    {formatCnpj(company.cnpj)}
                  </MuiLink>
                </TableCell>
                <TableCell>{company.legalName}</TableCell>
                <TableCell>{company.tradeName ?? '—'}</TableCell>
                <TableCell>{company.stateCode ?? '—'}</TableCell>
                <TableCell>{company.municipality ?? '—'}</TableCell>
              </TableRow>
            ))}
            emptyTitle="Nenhuma holding encontrada"
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
