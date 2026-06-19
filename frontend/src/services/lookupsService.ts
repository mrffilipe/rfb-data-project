import { api } from '../config'
import type { LookupItem, LookupSearchParams, PagedResult } from '../types'
import { normalizeLookupItem } from '../utils/apiMappers'
import { unwrapArray, unwrapPagedResult } from '../utils/apiResponse'
import { apiPaths } from './httpPaths'

async function searchLookup(path: string, params: LookupSearchParams): Promise<PagedResult<LookupItem>> {
  const { data } = await api.get(path, {
    params: {
      query: params.query,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  })
  return unwrapPagedResult(data, normalizeLookupItem)
}

async function listStaticLookup(path: string): Promise<LookupItem[]> {
  const { data } = await api.get(path)
  return unwrapArray(data, normalizeLookupItem)
}

export function listStates(): Promise<LookupItem[]> {
  return listStaticLookup(`${apiPaths.lookups}/states`)
}

export function listRegistrationStatuses(): Promise<LookupItem[]> {
  return listStaticLookup(`${apiPaths.lookups}/registration-statuses`)
}

export function listCompanySizes(): Promise<LookupItem[]> {
  return listStaticLookup(`${apiPaths.lookups}/company-sizes`)
}

export function searchCnaes(params: LookupSearchParams): Promise<PagedResult<LookupItem>> {
  return searchLookup(`${apiPaths.lookups}/cnaes`, params)
}

export function searchMunicipalities(params: LookupSearchParams): Promise<PagedResult<LookupItem>> {
  return searchLookup(`${apiPaths.lookups}/municipalities`, params)
}

export function searchLegalNatures(params: LookupSearchParams): Promise<PagedResult<LookupItem>> {
  return searchLookup(`${apiPaths.lookups}/legal-natures`, params)
}

export function searchQualifications(params: LookupSearchParams): Promise<PagedResult<LookupItem>> {
  return searchLookup(`${apiPaths.lookups}/qualifications`, params)
}
