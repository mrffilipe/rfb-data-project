import SearchIcon from '@mui/icons-material/Search'
import { Button, Link as MuiLink, Stack, TableCell, TableRow } from '@mui/material'
import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router'
import {
  CheckboxField,
  CompanyExportPanel,
  DataTable,
  FeedbackAlerts,
  FilterField,
  FormActions,
  FormGrid,
  FormGridItem,
  FormSection,
  LookupAutocomplete,
  PageHeader,
  SectionCard,
  TablePaginationBar,
} from '../components/ui'
import {
  listCnaes,
  listLegalNatures,
  listRegistrationStatuses,
  listStates,
  exportCompaniesList,
  listCompanies,
} from '../services'
import type { CompanySummary, ExportListCompaniesParams } from '../types'
import { emptyBooleanFilter, emptyLookupFilter, lookupCodes } from '../types'
import { getApiErrorMessage } from '../utils/apiError'
import { formatCnpj, resolveLookupDescription, toLookupLabelMap } from '../utils/formatters'

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
  const [stateFilter, setStateFilter] = useState(emptyLookupFilter)
  const [cnaeFilter, setCnaeFilter] = useState(emptyLookupFilter)
  const [legalNatureFilter, setLegalNatureFilter] = useState(emptyLookupFilter)
  const [headOfficeFilter, setHeadOfficeFilter] = useState(emptyBooleanFilter)
  const [registrationStatusLabels, setRegistrationStatusLabels] = useState<Map<string, string>>(new Map())

  const [items, setItems] = useState<CompanySummary[]>([])
  const [page, setPage] = useState(1)
  const [total, setTotal] = useState(0)
  const [totalIsApproximate, setTotalIsApproximate] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searched, setSearched] = useState(false)

  const loadStates = useCallback(() => listStates(), [])
  const loadCnaes = useCallback(() => listCnaes(), [])
  const loadLegalNatures = useCallback(() => listLegalNatures(), [])

  useEffect(() => {
    void listRegistrationStatuses().then((statuses) => {
      setRegistrationStatusLabels(toLookupLabelMap(statuses))
    })
  }, [])

  const buildExportParams = useCallback(
    (): Omit<ExportListCompaniesParams, 'limit' | 'deduplicateEmail' | 'page' | 'pageSize'> => ({
      stateCodes: lookupCodes(stateFilter),
      excludeStates: stateFilter.exclude,
      cnaes: lookupCodes(cnaeFilter),
      excludeCnaes: cnaeFilter.exclude,
      legalNatureCodes: lookupCodes(legalNatureFilter),
      excludeLegalNatureCodes: legalNatureFilter.exclude,
      headOfficeOnly: headOfficeFilter.value || undefined,
      excludeHeadOfficeOnly: headOfficeFilter.exclude && headOfficeFilter.value,
    }),
    [stateFilter, cnaeFilter, legalNatureFilter, headOfficeFilter],
  )

  async function runSearch(targetPage: number): Promise<void> {
    setLoading(true)
    setError(null)
    try {
      const result = await listCompanies({
        ...buildExportParams(),
        page: targetPage,
        pageSize: PAGE_SIZE,
      })
      setItems(result.items)
      setTotal(result.total)
      setTotalIsApproximate(result.totalIsApproximate ?? false)
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
        <Stack component="form" onSubmit={handleSubmit} spacing={3}>
          <FormSection title="Localização" description="Estado do estabelecimento">
            <FormGrid>
              <FormGridItem>
                <FilterField
                  helperText="UF onde o estabelecimento está registrado."
                  exclude={stateFilter.exclude}
                  onExcludeChange={(exclude) =>
                    setStateFilter((current) => ({ ...current, exclude }))
                  }
                >
                  <LookupAutocomplete
                    multiple
                    label="UF"
                    placeholder="Selecione um ou mais estados"
                    value={stateFilter.values}
                    onChange={(values) =>
                      setStateFilter((current) => ({ ...current, values }))
                    }
                    loadOptions={loadStates}
                  />
                </FilterField>
              </FormGridItem>
            </FormGrid>
          </FormSection>

          <FormSection title="Atividade econômica" description="Atividade principal (CNAE fiscal)" dividerBefore>
            <FormGrid>
              <FormGridItem md={12}>
                <FilterField
                  helperText="CNAE fiscal principal do estabelecimento."
                  exclude={cnaeFilter.exclude}
                  onExcludeChange={(exclude) =>
                    setCnaeFilter((current) => ({ ...current, exclude }))
                  }
                >
                  <LookupAutocomplete
                    multiple
                    loadOnOpen
                    label="CNAE"
                    placeholder="Selecione um ou mais CNAEs"
                    value={cnaeFilter.values}
                    onChange={(values) =>
                      setCnaeFilter((current) => ({ ...current, values }))
                    }
                    loadOptions={loadCnaes}
                  />
                </FilterField>
              </FormGridItem>
            </FormGrid>
          </FormSection>

          <FormSection title="Perfil da empresa" description="Tipo jurídico da empresa" dividerBefore>
            <FormGrid>
              <FormGridItem md={12}>
                <FilterField
                  helperText="Código de natureza jurídica da empresa."
                  exclude={legalNatureFilter.exclude}
                  onExcludeChange={(exclude) =>
                    setLegalNatureFilter((current) => ({ ...current, exclude }))
                  }
                >
                  <LookupAutocomplete
                    multiple
                    loadOnOpen
                    label="Natureza jurídica"
                    placeholder="Selecione uma ou mais naturezas"
                    value={legalNatureFilter.values}
                    onChange={(values) =>
                      setLegalNatureFilter((current) => ({ ...current, values }))
                    }
                    loadOptions={loadLegalNatures}
                  />
                </FilterField>
              </FormGridItem>
            </FormGrid>
          </FormSection>

          <FormSection title="Estabelecimento" description="Apenas sede (CNPJ matriz)" dividerBefore>
            <FilterField
              helperText="Restringe a busca à matriz da empresa."
              exclude={headOfficeFilter.exclude}
              onExcludeChange={(exclude) =>
                setHeadOfficeFilter((current) => ({ ...current, exclude }))
              }
            >
              <CheckboxField
                label="Somente matriz"
                checked={headOfficeFilter.value}
                onCheckedChange={(value) =>
                  setHeadOfficeFilter((current) => ({ ...current, value }))
                }
              />
            </FilterField>
          </FormSection>

          <FormActions>
            <Button type="submit" variant="contained" startIcon={<SearchIcon />} disabled={loading}>
              Listar
            </Button>
          </FormActions>
        </Stack>
      </SectionCard>

      {searched ? (
        <SectionCard title="Resultados">
          <CompanyExportPanel
            buildExportParams={buildExportParams}
            exportFn={exportCompaniesList}
            disabled={loading}
          />
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
            totalIsApproximate={totalIsApproximate}
            itemsOnPage={items.length}
            onPageChange={(nextPage) => void runSearch(nextPage)}
            disabled={loading}
          />
        </SectionCard>
      ) : null}
    </Stack>
  )
}
