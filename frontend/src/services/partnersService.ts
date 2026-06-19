import { api } from '../config'
import type { CompanySummary, GetCompaniesByPartnerParams, GetPartnersByCnpjParams, PagedResult, Partner } from '../types'
import { normalizeCompanySummary, normalizePartner } from '../utils/apiMappers'
import { unwrapPagedResult } from '../utils/apiResponse'
import { apiPaths } from './httpPaths'

export async function getCompaniesByPartner(
  params: GetCompaniesByPartnerParams,
): Promise<PagedResult<CompanySummary>> {
  const encoded = encodeURIComponent(params.document.replace(/\D/g, ''))
  const { data } = await api.get(`${apiPaths.partners}/${encoded}/companies`, {
    params: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  })
  return unwrapPagedResult(data, normalizeCompanySummary)
}

export async function getPartnersByCnpj(params: GetPartnersByCnpjParams): Promise<PagedResult<Partner>> {
  const encoded = encodeURIComponent(params.cnpj.replace(/\D/g, ''))
  const { data } = await api.get(`${apiPaths.partners}/by-cnpj/${encoded}`, {
    params: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  })
  return unwrapPagedResult(data, normalizePartner)
}
