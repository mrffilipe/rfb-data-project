export interface IngestionArtifactStatus {
  fileName: string
  targetTable: string
  status: string
  remoteSize?: number | null
  loadedAt?: string | null
}

export interface IngestionStatus {
  isSyncRunning: boolean
  isDataReady: boolean
  activeReferencePeriod?: string | null
  latestReferencePeriod?: string | null
  releaseStatus?: string | null
  totalArtifacts: number
  loadedArtifacts: number
  failedArtifacts: number
  lastSyncStartedAt?: string | null
  lastSyncCompletedAt?: string | null
  lastError?: string | null
  artifacts: IngestionArtifactStatus[]
}
