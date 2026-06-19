import { api } from '../config'
import type { CorporateParticipation, ListCorporateParticipationsParams, PagedResult } from '../types'
import { normalizeCorporateParticipation } from '../utils/apiMappers'
import { unwrapPagedResult } from '../utils/apiResponse'
import { apiPaths } from './httpPaths'

export async function listCorporateParticipations(
  params: ListCorporateParticipationsParams = {},
): Promise<PagedResult<CorporateParticipation>> {
  const { data } = await api.get(`${apiPaths.participations}/corporate`, {
    params: {
      controllingCnpj: params.controllingCnpj || undefined,
      controlledCnpj: params.controlledCnpj || undefined,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  })
  return unwrapPagedResult(data, normalizeCorporateParticipation)
}
