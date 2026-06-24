import { api } from '../config'
import type {
  CompanyDetail,
  CompanySummary,
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
