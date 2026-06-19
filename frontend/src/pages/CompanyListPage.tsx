import SearchIcon from '@mui/icons-material/Search'
import { Button, Link as MuiLink, Stack, TableCell, TableRow } from '@mui/material'
import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router'
import {
  CheckboxField,
  DataTable,
  FeedbackAlerts,
  FormActions,
  FormGrid,
  FormGridItem,
  LookupAutocomplete,
  PageHeader,
  PaginatedAutocomplete,
  SectionCard,
  TablePaginationBar,
} from '../components/ui'
import { listCompanies, listRegistrationStatuses, listStates, searchCnaes, searchLegalNatures } from '../services'
import type { CompanySummary, LookupItem } from '../types'
import { getApiErrorMessage } from '../utils/apiError'
import { formatCnpj, formatLookupLabel, resolveLookupDescription, toLookupLabelMap } from '../utils/formatters'

const PAGE_SIZE = 20

const companyColumns = [
  { id: 'cnpj', label: 'CNPJ', minWidth: 140 },
  { id: 'legalName', label: 'Razão social', minWidth: 200 },
  { id: 'tradeName', label: 'Fantasia', minWidth: 160 },
  { id: 'stateCode', label: 'UF' },
  { id: 'municipality', label: 'Município', minWidth: 140 },
  { id: 'status', label: 'Situação' },
]

export function CompanyListPage() {
  const [state, setState] = useState<LookupItem | null>(null)
  const [cnae, setCnae] = useState<LookupItem | null>(null)
  const [legalNature, setLegalNature] = useState<LookupItem | null>(null)
  const [headOfficeOnly, setHeadOfficeOnly] = useState(false)
  const [registrationStatusLabels, setRegistrationStatusLabels] = useState<Map<string, string>>(new Map())

  const [items, setItems] = useState<CompanySummary[]>([])
  const [page, setPage] = useState(1)
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searched, setSearched] = useState(false)

  const loadStates = useCallback(() => listStates(), [])

  const fetchCnaes = useCallback(
    (searchQuery: string, pageNumber: number) =>
      searchCnaes({ query: searchQuery, page: pageNumber, pageSize: 20 }),
    [],
  )

  const fetchLegalNatures = useCallback(
    (searchQuery: string, pageNumber: number) =>
      searchLegalNatures({ query: searchQuery, page: pageNumber, pageSize: 20 }),
    [],
  )

  useEffect(() => {
    void listRegistrationStatuses().then((statuses) => {
      setRegistrationStatusLabels(toLookupLabelMap(statuses))
    })
  }, [])

  async function runSearch(targetPage: number): Promise<void> {
    setLoading(true)
    setError(null)
    try {
      const result = await listCompanies({
        stateCode: state?.code,
        cnae: cnae?.code,
        legalNatureCode: legalNature?.code,
        headOfficeOnly,
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
        title="Listagem de empresas"
        description="Lista paginada de empresas com filtros básicos."
      />
      <FeedbackAlerts error={error} />

      <SectionCard title="Filtros">
        <Stack component="form" onSubmit={handleSubmit} spacing={2}>
          <FormGrid>
            <FormGridItem>
              <LookupAutocomplete
                label="UF"
                placeholder="Buscar estado"
                value={state}
                onChange={setState}
                loadOptions={loadStates}
              />
            </FormGridItem>
            <FormGridItem>
              <PaginatedAutocomplete
                label="CNAE"
                value={cnae}
                onChange={setCnae}
                fetchPage={fetchCnaes}
                getOptionLabel={formatLookupLabel}
                isOptionEqualToValue={(a, b) => a.code === b.code}
              />
            </FormGridItem>
            <FormGridItem>
              <PaginatedAutocomplete
                label="Natureza jurídica"
                value={legalNature}
                onChange={setLegalNature}
                fetchPage={fetchLegalNatures}
                getOptionLabel={formatLookupLabel}
                isOptionEqualToValue={(a, b) => a.code === b.code}
              />
            </FormGridItem>
            <FormGridItem md={12}>
              <CheckboxField
                label="Somente matriz"
                checked={headOfficeOnly}
                onCheckedChange={setHeadOfficeOnly}
              />
            </FormGridItem>
          </FormGrid>
          <FormActions>
            <Button type="submit" variant="contained" startIcon={<SearchIcon />} disabled={loading}>
              Listar
            </Button>
          </FormActions>
        </Stack>
      </SectionCard>

      {searched ? (
        <SectionCard title="Resultados">
          <DataTable
            columns={companyColumns}
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
                <TableCell>
                  {resolveLookupDescription(company.registrationStatus, registrationStatusLabels)}
                </TableCell>
              </TableRow>
            ))}
            emptyTitle="Nenhuma empresa encontrada"
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
