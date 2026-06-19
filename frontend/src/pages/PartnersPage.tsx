import SearchIcon from '@mui/icons-material/Search'
import { Button, Link as MuiLink, Stack, Tab, TableCell, TableRow, Tabs, TextField } from '@mui/material'
import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router'
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
import { getCompaniesByPartner, getPartnersByCnpj } from '../services'
import type { CompanySummary, Partner } from '../types'
import { getApiErrorMessage } from '../utils/apiError'
import { formatCnpj, formatDocument } from '../utils/formatters'

const PAGE_SIZE = 20

export function PartnersPage() {
  const [searchParams] = useSearchParams()
  const initialCnpj = searchParams.get('cnpj') ?? ''
  const [tab, setTab] = useState(initialCnpj ? 1 : 0)

  const [document, setDocument] = useState('')
  const [cnpj, setCnpj] = useState(initialCnpj)

  const [companies, setCompanies] = useState<CompanySummary[]>([])
  const [partners, setPartners] = useState<Partner[]>([])
  const [page, setPage] = useState(1)
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searched, setSearched] = useState(false)

  useEffect(() => {
    if (initialCnpj) {
      setCnpj(initialCnpj)
      setTab(1)
      void runPartnersSearch(1, initialCnpj)
    }
  }, [initialCnpj])

  async function runCompaniesSearch(targetPage: number): Promise<void> {
    if (!document.trim()) {
      setError('Informe o documento do sócio.')
      return
    }
    setLoading(true)
    setError(null)
    try {
      const result = await getCompaniesByPartner({
        document: document.trim(),
        page: targetPage,
        pageSize: PAGE_SIZE,
      })
      setCompanies(result.items)
      setTotal(result.total)
      setPage(result.page)
      setSearched(true)
    } catch (searchError) {
      setError(getApiErrorMessage(searchError))
    } finally {
      setLoading(false)
    }
  }

  async function runPartnersSearch(targetPage: number, cnpjValue = cnpj): Promise<void> {
    if (!cnpjValue.trim()) {
      setError('Informe o CNPJ da empresa.')
      return
    }
    setLoading(true)
    setError(null)
    try {
      const result = await getPartnersByCnpj({
        cnpj: cnpjValue.trim(),
        page: targetPage,
        pageSize: PAGE_SIZE,
      })
      setPartners(result.items)
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
    if (tab === 0) {
      await runCompaniesSearch(1)
    } else {
      await runPartnersSearch(1)
    }
  }

  async function handlePageChange(nextPage: number): Promise<void> {
    if (tab === 0) {
      await runCompaniesSearch(nextPage)
    } else {
      await runPartnersSearch(nextPage)
    }
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Consulta de sócios"
        description="Busque empresas por documento do sócio ou liste sócios de um CNPJ."
      />
      <FeedbackAlerts error={error} />

      <Tabs value={tab} onChange={(_e, value) => { setTab(value); setSearched(false) }}>
        <Tab label="Empresas por documento do sócio" />
        <Tab label="Sócios por CNPJ" />
      </Tabs>

      <SectionCard title="Filtros">
        <Stack component="form" onSubmit={handleSubmit} spacing={2}>
          <FormGrid>
            {tab === 0 ? (
              <FormGridItem md={6}>
                <TextField
                  label="CPF/CNPJ do sócio"
                  value={document}
                  onChange={(e) => setDocument(e.target.value)}
                  fullWidth
                  required
                />
              </FormGridItem>
            ) : (
              <FormGridItem md={6}>
                <TextField
                  label="CNPJ da empresa"
                  value={cnpj}
                  onChange={(e) => setCnpj(e.target.value)}
                  fullWidth
                  required
                />
              </FormGridItem>
            )}
          </FormGrid>
          <FormActions>
            <Button type="submit" variant="contained" startIcon={<SearchIcon />} disabled={loading}>
              Buscar
            </Button>
          </FormActions>
        </Stack>
      </SectionCard>

      {searched && tab === 0 ? (
        <SectionCard title="Empresas">
          <DataTable
            columns={[
              { id: 'cnpj', label: 'CNPJ', minWidth: 140 },
              { id: 'legalName', label: 'Razão social', minWidth: 200 },
              { id: 'tradeName', label: 'Fantasia' },
              { id: 'stateCode', label: 'UF' },
            ]}
            loading={loading}
            rows={companies.map((company) => (
              <TableRow key={company.cnpj} hover>
                <TableCell>
                  <MuiLink component={Link} to={`/companies/${company.cnpj}`} underline="hover">
                    {formatCnpj(company.cnpj)}
                  </MuiLink>
                </TableCell>
                <TableCell>{company.legalName}</TableCell>
                <TableCell>{company.tradeName ?? '—'}</TableCell>
                <TableCell>{company.stateCode ?? '—'}</TableCell>
              </TableRow>
            ))}
            emptyTitle="Nenhuma empresa encontrada"
          />
          <TablePaginationBar
            page={page}
            pageSize={PAGE_SIZE}
            total={total}
            onPageChange={(nextPage) => void handlePageChange(nextPage)}
            disabled={loading}
          />
        </SectionCard>
      ) : null}

      {searched && tab === 1 ? (
        <SectionCard title="Sócios">
          <DataTable
            columns={[
              { id: 'name', label: 'Nome', minWidth: 200 },
              { id: 'document', label: 'Documento', minWidth: 140 },
              { id: 'type', label: 'Tipo' },
              { id: 'qualification', label: 'Qualificação' },
              { id: 'entryDate', label: 'Entrada' },
            ]}
            loading={loading}
            rows={partners.map((partner, index) => (
              <TableRow key={`${partner.cnpjBase}-${partner.partnerDocument ?? index}`} hover>
                <TableCell>{partner.partnerName}</TableCell>
                <TableCell>
                  {partner.partnerDocument ? formatDocument(partner.partnerDocument) : '—'}
                </TableCell>
                <TableCell>{partner.partnerTypeIdentifier ?? '—'}</TableCell>
                <TableCell>{partner.partnerQualificationCode ?? '—'}</TableCell>
                <TableCell>{partner.entryDate ?? '—'}</TableCell>
              </TableRow>
            ))}
            emptyTitle="Nenhum sócio encontrado"
          />
          <TablePaginationBar
            page={page}
            pageSize={PAGE_SIZE}
            total={total}
            onPageChange={(nextPage) => void handlePageChange(nextPage)}
            disabled={loading}
          />
        </SectionCard>
      ) : null}
    </Stack>
  )
}
