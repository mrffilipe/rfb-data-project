import type {
  CompanyDetail,
  CompanySummary,
  CorporateParticipation,
  IngestionArtifactStatus,
  IngestionStatus,
  LookupItem,
  Partner,
} from '../types'

function asRecord(value: unknown): Record<string, unknown> {
  return value !== null && typeof value === 'object' && !Array.isArray(value)
    ? (value as Record<string, unknown>)
    : {}
}

function str(value: unknown): string | null | undefined {
  if (value === null || value === undefined) {
    return value as null | undefined
  }
  return String(value)
}

function num(value: unknown): number {
  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : 0
}

export function normalizeCompanySummary(raw: unknown): CompanySummary {
  const r = asRecord(raw)
  return {
    cnpj: String(r.cnpj ?? r.Cnpj ?? ''),
    legalName: String(r.legalName ?? r.LegalName ?? ''),
    tradeName: str(r.tradeName ?? r.TradeName),
    stateCode: str(r.stateCode ?? r.StateCode),
    municipality: str(r.municipality ?? r.Municipality),
    primaryCnaeCode: str(r.primaryCnaeCode ?? r.PrimaryCnaeCode),
    primaryCnaeDescription: str(r.primaryCnaeDescription ?? r.PrimaryCnaeDescription),
    registrationStatus: str(r.registrationStatus ?? r.RegistrationStatus),
  }
}

export function normalizeCompanyDetail(raw: unknown): CompanyDetail {
  const r = asRecord(raw)
  return {
    cnpj: String(r.cnpj ?? r.Cnpj ?? ''),
    legalName: String(r.legalName ?? r.LegalName ?? ''),
    tradeName: str(r.tradeName ?? r.TradeName),
    legalNatureCode: str(r.legalNatureCode ?? r.LegalNatureCode),
    legalNatureDescription: str(r.legalNatureDescription ?? r.LegalNatureDescription),
    shareCapital: str(r.shareCapital ?? r.ShareCapital),
    companySizeCode: str(r.companySizeCode ?? r.CompanySizeCode),
    registrationStatus: str(r.registrationStatus ?? r.RegistrationStatus),
    activityStartDate: str(r.activityStartDate ?? r.ActivityStartDate),
    primaryCnaeCode: str(r.primaryCnaeCode ?? r.PrimaryCnaeCode),
    primaryCnaeDescription: str(r.primaryCnaeDescription ?? r.PrimaryCnaeDescription),
    stateCode: str(r.stateCode ?? r.StateCode),
    municipalityCode: str(r.municipalityCode ?? r.MunicipalityCode),
    municipalityDescription: str(r.municipalityDescription ?? r.MunicipalityDescription),
    streetName: str(r.streetName ?? r.StreetName),
    streetNumber: str(r.streetNumber ?? r.StreetNumber),
    neighborhood: str(r.neighborhood ?? r.Neighborhood),
    zipCode: str(r.zipCode ?? r.ZipCode),
    email: str(r.email ?? r.Email),
    phoneNumber: str(r.phoneNumber ?? r.PhoneNumber),
  }
}

export function normalizePartner(raw: unknown): Partner {
  const r = asRecord(raw)
  return {
    cnpjBase: String(r.cnpjBase ?? r.CnpjBase ?? ''),
    partnerName: String(r.partnerName ?? r.PartnerName ?? ''),
    partnerTypeIdentifier: str(r.partnerTypeIdentifier ?? r.PartnerTypeIdentifier),
    partnerDocument: str(r.partnerDocument ?? r.PartnerDocument),
    partnerQualificationCode: str(r.partnerQualificationCode ?? r.PartnerQualificationCode),
    entryDate: str(r.entryDate ?? r.EntryDate),
  }
}

export function normalizeCorporateParticipation(raw: unknown): CorporateParticipation {
  const r = asRecord(raw)
  return {
    controlledCnpjBase: String(r.controlledCnpjBase ?? r.ControlledCnpjBase ?? ''),
    controllingCnpj: String(r.controllingCnpj ?? r.ControllingCnpj ?? ''),
    controllingLegalName: str(r.controllingLegalName ?? r.ControllingLegalName),
    partnerQualificationCode: str(r.partnerQualificationCode ?? r.PartnerQualificationCode),
    entryDate: str(r.entryDate ?? r.EntryDate),
    controlledLegalName: str(r.controlledLegalName ?? r.ControlledLegalName),
  }
}

export function normalizeLookupItem(raw: unknown): LookupItem {
  const r = asRecord(raw)
  return {
    code: String(r.code ?? r.Code ?? ''),
    description: String(r.description ?? r.Description ?? ''),
  }
}

function normalizeIngestionArtifact(raw: unknown): IngestionArtifactStatus {
  const r = asRecord(raw)
  return {
    fileName: String(r.fileName ?? r.FileName ?? ''),
    targetTable: String(r.targetTable ?? r.TargetTable ?? ''),
    status: String(r.status ?? r.Status ?? ''),
    remoteSize: r.remoteSize != null || r.RemoteSize != null ? num(r.remoteSize ?? r.RemoteSize) : null,
    loadedAt: str(r.loadedAt ?? r.LoadedAt),
  }
}

export function normalizeIngestionStatus(raw: unknown): IngestionStatus {
  const r = asRecord(raw)
  const artifactsRaw = r.artifacts ?? r.Artifacts
  const artifacts = Array.isArray(artifactsRaw) ? artifactsRaw.map(normalizeIngestionArtifact) : []

  return {
    isSyncRunning: Boolean(r.isSyncRunning ?? r.IsSyncRunning),
    isDataReady: Boolean(r.isDataReady ?? r.IsDataReady),
    activeReferencePeriod: str(r.activeReferencePeriod ?? r.ActiveReferencePeriod),
    latestReferencePeriod: str(r.latestReferencePeriod ?? r.LatestReferencePeriod),
    releaseStatus: str(r.releaseStatus ?? r.ReleaseStatus),
    totalArtifacts: num(r.totalArtifacts ?? r.TotalArtifacts),
    loadedArtifacts: num(r.loadedArtifacts ?? r.LoadedArtifacts),
    failedArtifacts: num(r.failedArtifacts ?? r.FailedArtifacts),
    lastSyncStartedAt: str(r.lastSyncStartedAt ?? r.LastSyncStartedAt),
    lastSyncCompletedAt: str(r.lastSyncCompletedAt ?? r.LastSyncCompletedAt),
    lastError: str(r.lastError ?? r.LastError),
    artifacts,
  }
}
