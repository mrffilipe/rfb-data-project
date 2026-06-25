import { api, env } from '../config'
import type {
  CompanyDetail,
  CompanyExportResult,
  CompanySummary,
  ExportListCompaniesParams,
  ExportSearchCompaniesParams,
  ListCompaniesParams,
  ListHoldingsParams,
  PagedResult,
  SearchCompaniesParams,
} from '../types'
import { normalizeCompanyDetail, normalizeCompanySummary } from '../utils/apiMappers'
import { unwrapPagedResult } from '../utils/apiResponse'
import { serializeQueryParams } from '../utils/queryParams'
import { apiPaths } from './httpPaths'

export async function getCompanyByCnpj(cnpj: string): Promise<CompanyDetail> {
  const encoded = encodeURIComponent(cnpj.replace(/\D/g, ''))
  const { data } = await api.get(`${apiPaths.companies}/${encoded}`)
  return normalizeCompanyDetail(data)
}

function buildCompanyFilterParams(
  params: SearchCompaniesParams | ListCompaniesParams,
): Record<string, unknown> {
  const base: Record<string, unknown> = {
    page: params.page ?? 1,
    pageSize: params.pageSize ?? 20,
  }

  if ('query' in params && params.query) {
    base.query = params.query
    if (params.excludeQuery) base.excludeQuery = true
  }

  if (params.stateCodes?.length) {
    base.stateCodes = params.stateCodes
    if (params.excludeStates) base.excludeStates = true
  }

  if (params.cnaes?.length) {
    base.cnaes = params.cnaes
    if (params.excludeCnaes) base.excludeCnaes = true
  }

  if (params.legalNatureCodes?.length) {
    base.legalNatureCodes = params.legalNatureCodes
    if (params.excludeLegalNatureCodes) base.excludeLegalNatureCodes = true
  }

  if ('companySizeCodes' in params && params.companySizeCodes?.length) {
    base.companySizeCodes = params.companySizeCodes
    if (params.excludeCompanySizes) base.excludeCompanySizes = true
  }

  if ('registrationStatuses' in params && params.registrationStatuses?.length) {
    base.registrationStatuses = params.registrationStatuses
    if (params.excludeRegistrationStatuses) base.excludeRegistrationStatuses = true
  }

  if (params.headOfficeOnly) {
    base.headOfficeOnly = true
    if (params.excludeHeadOfficeOnly) base.excludeHeadOfficeOnly = true
  }

  if ('shareCapitalMin' in params && params.shareCapitalMin !== undefined) {
    base.shareCapitalMin = params.shareCapitalMin
  }

  if ('shareCapitalMax' in params && params.shareCapitalMax !== undefined) {
    base.shareCapitalMax = params.shareCapitalMax
  }

  if ('excludeShareCapitalRange' in params && params.excludeShareCapitalRange) {
    base.excludeShareCapitalRange = true
  }

  return base
}

function resolveExportTimeout(limit: number): number {
  return Math.min(600_000, Math.max(env.apiTimeoutMs, 60_000 + limit * 30))
}

function buildExportParams(
  params: ExportSearchCompaniesParams | ExportListCompaniesParams,
): Record<string, unknown> {
  const { limit = 100, deduplicateEmail, page: _page, pageSize: _pageSize, ...filters } = params
  return {
    ...buildCompanyFilterParams({ ...filters, page: 1, pageSize: 20 }),
    limit,
    deduplicateEmail: deduplicateEmail || undefined,
  }
}

function normalizeExportRow(raw: unknown): Record<string, string | null> {
  if (raw === null || typeof raw !== 'object' || Array.isArray(raw)) {
    return {}
  }

  const record = raw as Record<string, unknown>
  const normalized: Record<string, string | null> = {}
  for (const [key, value] of Object.entries(record)) {
    normalized[key] = value === null || value === undefined ? null : String(value)
  }
  return normalized
}

function normalizeExportStats(raw: unknown): CompanyExportResult['stats'] {
  const r = raw as Record<string, unknown>
  return {
    requestedLimit: Number(r.requestedLimit ?? r.RequestedLimit ?? 0),
    exportedCount: Number(r.exportedCount ?? r.ExportedCount ?? 0),
    scannedCount: Number(r.scannedCount ?? r.ScannedCount ?? 0),
    withEmailCount: Number(r.withEmailCount ?? r.WithEmailCount ?? 0),
    withoutEmailCount: Number(r.withoutEmailCount ?? r.WithoutEmailCount ?? 0),
    uniqueEmailCount: Number(r.uniqueEmailCount ?? r.UniqueEmailCount ?? 0),
    duplicateEmailSkippedCount: Number(
      r.duplicateEmailSkippedCount ?? r.DuplicateEmailSkippedCount ?? 0,
    ),
  }
}

function normalizeCompanyExportResult(raw: unknown): CompanyExportResult {
  const r = raw as Record<string, unknown>
  const columnsRaw = r.columns ?? r.Columns
  const itemsRaw = r.items ?? r.Items
  const statsRaw = r.stats ?? r.Stats

  const columns = Array.isArray(columnsRaw) ? columnsRaw.map(String) : []
  const items = Array.isArray(itemsRaw) ? itemsRaw.map(normalizeExportRow) : []

  return {
    columns,
    stats: normalizeExportStats(statsRaw),
    items,
  }
}

export async function exportCompaniesSearch(
  params: ExportSearchCompaniesParams = {},
): Promise<CompanyExportResult> {
  const limit = params.limit ?? 100
  const { data } = await api.get(`${apiPaths.companies}/search/export`, {
    params: buildExportParams(params),
    paramsSerializer: { serialize: serializeQueryParams },
    timeout: resolveExportTimeout(limit),
  })
  return normalizeCompanyExportResult(data)
}

export async function exportCompaniesList(
  params: ExportListCompaniesParams = {},
): Promise<CompanyExportResult> {
  const limit = params.limit ?? 100
  const { data } = await api.get(`${apiPaths.companies}/export`, {
    params: buildExportParams(params),
    paramsSerializer: { serialize: serializeQueryParams },
    timeout: resolveExportTimeout(limit),
  })
  return normalizeCompanyExportResult(data)
}

export async function searchCompanies(params: SearchCompaniesParams = {}): Promise<PagedResult<CompanySummary>> {
  const { data } = await api.get(`${apiPaths.companies}/search`, {
    params: buildCompanyFilterParams(params),
    paramsSerializer: { serialize: serializeQueryParams },
  })
  return unwrapPagedResult(data, normalizeCompanySummary)
}

export async function listCompanies(params: ListCompaniesParams = {}): Promise<PagedResult<CompanySummary>> {
  const { data } = await api.get(apiPaths.companies, {
    params: buildCompanyFilterParams(params),
    paramsSerializer: { serialize: serializeQueryParams },
  })
  return unwrapPagedResult(data, normalizeCompanySummary)
}

export async function listHoldings(params: ListHoldingsParams = {}): Promise<PagedResult<CompanySummary>> {
  const { data } = await api.get(`${apiPaths.companies}/holdings`, {
    params: {
      stateCode: params.stateCode || undefined,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  })
  return unwrapPagedResult(data, normalizeCompanySummary)
}
