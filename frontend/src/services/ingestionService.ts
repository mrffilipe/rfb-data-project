import { api } from '../config'
import type { IngestionStatus } from '../types'
import { normalizeIngestionStatus } from '../utils/apiMappers'
import { apiPaths } from './httpPaths'

export async function getIngestionStatus(): Promise<IngestionStatus> {
  const { data } = await api.get(`${apiPaths.ingestion}/status`)
  return normalizeIngestionStatus(data)
}

export async function triggerIngestionSync(): Promise<IngestionStatus> {
  const { data } = await api.post(`${apiPaths.ingestion}/sync`)
  return normalizeIngestionStatus(data)
}
