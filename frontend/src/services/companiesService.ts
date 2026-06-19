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
import { apiPaths } from './httpPaths'

export async function getCompanyByCnpj(cnpj: string): Promise<CompanyDetail> {
  const encoded = encodeURIComponent(cnpj.replace(/\D/g, ''))
  const { data } = await api.get(`${apiPaths.companies}/${encoded}`)
  return normalizeCompanyDetail(data)
}

export async function searchCompanies(params: SearchCompaniesParams = {}): Promise<PagedResult<CompanySummary>> {
  const { data } = await api.get(`${apiPaths.companies}/search`, {
    params: {
      query: params.query || undefined,
      stateCode: params.stateCode || undefined,
      cnae: params.cnae || undefined,
      legalNatureCode: params.legalNatureCode || undefined,
      companySizeCode: params.companySizeCode || undefined,
      registrationStatus: params.registrationStatus || undefined,
      headOfficeOnly: params.headOfficeOnly || undefined,
      shareCapitalMin: params.shareCapitalMin,
      shareCapitalMax: params.shareCapitalMax,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  })
  return unwrapPagedResult(data, normalizeCompanySummary)
}

export async function listCompanies(params: ListCompaniesParams = {}): Promise<PagedResult<CompanySummary>> {
  const { data } = await api.get(apiPaths.companies, {
    params: {
      stateCode: params.stateCode || undefined,
      cnae: params.cnae || undefined,
      legalNatureCode: params.legalNatureCode || undefined,
      headOfficeOnly: params.headOfficeOnly || undefined,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
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
