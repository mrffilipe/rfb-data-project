import SearchIcon from '@mui/icons-material/Search'
import { Button, Link as MuiLink, Stack, TableCell, TableRow, TextField } from '@mui/material'
import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router'
import {
  CheckboxField,
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
  listCompanySizes,
  listLegalNatures,
  listRegistrationStatuses,
  listStates,
  searchCompanies,
} from '../services'
import type { CompanySummary } from '../types'
import {
  emptyBooleanFilter,
  emptyLookupFilter,
  emptyRangeFilter,
  emptyTextFilter,
  lookupCodes,
} from '../types'
import { getApiErrorMessage } from '../utils/apiError'
import { formatCnpj, resolveLookupDescription, toLookupLabelMap } from '../utils/formatters'

const PAGE_SIZE = 20

const companyColumns = [
  { id: 'cnpj', label: 'CNPJ', minWidth: 140 },
  { id: 'legalName', label: 'Razão social', minWidth: 200 },
  { id: 'tradeName', label: 'Fantasia', minWidth: 160 },
  { id: 'stateCode', label: 'UF' },
  { id: 'municipality', label: 'Município', minWidth: 140 },
  { id: 'cnae', label: 'CNAE', minWidth: 160 },
  { id: 'status', label: 'Situação' },
]

const PORTE_HELPER: Record<string, string> = {
  '4': 'Na base da Receita, consta como Demais.',
  '5': 'Na base da Receita, consta como Demais.',
}

export function CompanySearchPage() {
  const [textFilter, setTextFilter] = useState(emptyTextFilter)
  const [stateFilter, setStateFilter] = useState(emptyLookupFilter)
  const [cnaeFilter, setCnaeFilter] = useState(emptyLookupFilter)
  const [legalNatureFilter, setLegalNatureFilter] = useState(emptyLookupFilter)
  const [companySizeFilter, setCompanySizeFilter] = useState(emptyLookupFilter)
  const [registrationStatusFilter, setRegistrationStatusFilter] = useState(emptyLookupFilter)
  const [capitalFilter, setCapitalFilter] = useState(emptyRangeFilter)
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
  const loadCompanySizes = useCallback(() => listCompanySizes(), [])
  const loadRegistrationStatuses = useCallback(() => listRegistrationStatuses(), [])

  useEffect(() => {
    void listRegistrationStatuses().then((statuses) => {
      setRegistrationStatusLabels(toLookupLabelMap(statuses))
    })
  }, [])

  async function runSearch(targetPage: number): Promise<void> {
    setLoading(true)
    setError(null)
    try {
      const result = await searchCompanies({
        query: textFilter.value.trim() || undefined,
        excludeQuery: textFilter.exclude && Boolean(textFilter.value.trim()),
        stateCodes: lookupCodes(stateFilter),
        excludeStates: stateFilter.exclude,
        cnaes: lookupCodes(cnaeFilter),
        excludeCnaes: cnaeFilter.exclude,
        legalNatureCodes: lookupCodes(legalNatureFilter),
        excludeLegalNatureCodes: legalNatureFilter.exclude,
        companySizeCodes: lookupCodes(companySizeFilter),
        excludeCompanySizes: companySizeFilter.exclude,
        registrationStatuses: lookupCodes(registrationStatusFilter),
        excludeRegistrationStatuses: registrationStatusFilter.exclude,
        headOfficeOnly: headOfficeFilter.value || undefined,
        excludeHeadOfficeOnly: headOfficeFilter.exclude && headOfficeFilter.value,
        shareCapitalMin: capitalFilter.min ? Number(capitalFilter.min) : undefined,
        shareCapitalMax: capitalFilter.max ? Number(capitalFilter.max) : undefined,
        excludeShareCapitalRange:
          capitalFilter.exclude && Boolean(capitalFilter.min || capitalFilter.max),
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
        title="Busca de empresas"
        description="Pesquisa avançada com filtros por texto, localização, CNAE, natureza jurídica e capital social."
      />
      <FeedbackAlerts error={error} />

      <SectionCard title="Filtros de busca">
        <Stack component="form" onSubmit={handleSubmit} spacing={3}>
          <FormSection title="Busca por texto" description="Razão social, nome fantasia ou CNPJ">
            <FilterField
              helperText="Termo buscado no nome da empresa."
              exclude={textFilter.exclude}
              onExcludeChange={(exclude) => setTextFilter((current) => ({ ...current, exclude }))}
            >
              <TextField
                label="Texto livre"
                placeholder="Razão social, nome fantasia ou CNPJ"
                value={textFilter.value}
                onChange={(event) =>
                  setTextFilter((current) => ({ ...current, value: event.target.value }))
                }
                fullWidth
              />
            </FilterField>
          </FormSection>

          <FormSection title="Localização" description="Estado do estabelecimento" dividerBefore>
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

          <FormSection
            title="Perfil da empresa"
            description="Tipo jurídico, status na Receita e porte"
            dividerBefore
          >
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
              <FormGridItem>
                <FilterField
                  helperText="Situação cadastral na Receita Federal."
                  exclude={registrationStatusFilter.exclude}
                  onExcludeChange={(exclude) =>
                    setRegistrationStatusFilter((current) => ({ ...current, exclude }))
                  }
                >
                  <LookupAutocomplete
                    multiple
                    label="Situação cadastral"
                    placeholder="Selecione uma ou mais situações"
                    value={registrationStatusFilter.values}
                    onChange={(values) =>
                      setRegistrationStatusFilter((current) => ({ ...current, values }))
                    }
                    loadOptions={loadRegistrationStatuses}
                  />
                </FilterField>
              </FormGridItem>
              <FormGridItem>
                <FilterField
                  helperText="MEI, ME, EPP, médio ou grande porte."
                  exclude={companySizeFilter.exclude}
                  onExcludeChange={(exclude) =>
                    setCompanySizeFilter((current) => ({ ...current, exclude }))
                  }
                >
                  <LookupAutocomplete
                    multiple
                    label="Porte"
                    placeholder="Selecione um ou mais portes"
                    value={companySizeFilter.values}
                    onChange={(values) =>
                      setCompanySizeFilter((current) => ({ ...current, values }))
                    }
                    loadOptions={loadCompanySizes}
                    helperText={
                      companySizeFilter.values.some((item) => PORTE_HELPER[item.code])
                        ? PORTE_HELPER[companySizeFilter.values.find((item) => PORTE_HELPER[item.code])!.code]
                        : undefined
                    }
                  />
                </FilterField>
              </FormGridItem>
            </FormGrid>
          </FormSection>

          <FormSection title="Capital social" description="Valor do capital social declarado (R$)" dividerBefore>
            <FilterField
              helperText="Faixa de capital social em reais."
              exclude={capitalFilter.exclude}
              onExcludeChange={(exclude) =>
                setCapitalFilter((current) => ({ ...current, exclude }))
              }
            >
              <FormGrid>
                <FormGridItem md={6}>
                  <TextField
                    label="Capital social mínimo"
                    type="number"
                    value={capitalFilter.min}
                    onChange={(event) =>
                      setCapitalFilter((current) => ({ ...current, min: event.target.value }))
                    }
                    fullWidth
                  />
                </FormGridItem>
                <FormGridItem md={6}>
                  <TextField
                    label="Capital social máximo"
                    type="number"
                    value={capitalFilter.max}
                    onChange={(event) =>
                      setCapitalFilter((current) => ({ ...current, max: event.target.value }))
                    }
                    fullWidth
                  />
                </FormGridItem>
              </FormGrid>
            </FilterField>
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
              Buscar
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
                  {company.primaryCnaeCode
                    ? `${company.primaryCnaeCode}${company.primaryCnaeDescription ? ` — ${company.primaryCnaeDescription}` : ''}`
                    : '—'}
                </TableCell>
                <TableCell>
                  {resolveLookupDescription(company.registrationStatus, registrationStatusLabels)}
                </TableCell>
              </TableRow>
            ))}
            emptyTitle="Nenhuma empresa encontrada"
            emptyDescription="Ajuste os filtros e tente novamente."
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
