import { api } from '../config'
import { apiPaths } from './httpPaths'

export interface HealthStatus {
  status: string
}

export async function checkHealth(): Promise<HealthStatus> {
  const { data } = await api.get<HealthStatus>(apiPaths.health)
  return data
}
