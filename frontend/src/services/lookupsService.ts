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

const lookupListCache = new Map<string, Promise<LookupItem[]>>()

function cachedListLookup(path: string): Promise<LookupItem[]> {
  const cached = lookupListCache.get(path)
  if (cached) {
    return cached
  }

  const request = listStaticLookup(path).catch((error) => {
    lookupListCache.delete(path)
    throw error
  })
  lookupListCache.set(path, request)
  return request
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

export function listCnaes(): Promise<LookupItem[]> {
  return cachedListLookup(`${apiPaths.lookups}/cnaes/all`)
}

export function listLegalNatures(): Promise<LookupItem[]> {
  return cachedListLookup(`${apiPaths.lookups}/legal-natures/all`)
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
