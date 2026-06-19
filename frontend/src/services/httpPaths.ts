import { env } from '../config/env'

const versionPrefix = `/v${env.apiVersion}`

export const apiPaths = {
  versionPrefix,
  health: `${versionPrefix}/health`,
  companies: `${versionPrefix}/Companies`,
  partners: `${versionPrefix}/Partners`,
  participations: `${versionPrefix}/Participations`,
  lookups: `${versionPrefix}/Lookups`,
  ingestion: `${versionPrefix}/Ingestion`,
} as const
