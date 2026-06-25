export interface CompanySummary {
  cnpj: string
  legalName: string
  tradeName?: string | null
  stateCode?: string | null
  municipality?: string | null
  primaryCnaeCode?: string | null
  primaryCnaeDescription?: string | null
  registrationStatus?: string | null
}

export interface CompanyDetail {
  cnpj: string
  legalName: string
  tradeName?: string | null
  legalNatureCode?: string | null
  legalNatureDescription?: string | null
  shareCapital?: string | null
  companySizeCode?: string | null
  registrationStatus?: string | null
  activityStartDate?: string | null
  primaryCnaeCode?: string | null
  primaryCnaeDescription?: string | null
  stateCode?: string | null
  municipalityCode?: string | null
  municipalityDescription?: string | null
  streetName?: string | null
  streetNumber?: string | null
  neighborhood?: string | null
  zipCode?: string | null
  email?: string | null
  phoneNumber?: string | null
}

export interface SearchCompaniesParams {
  query?: string
  excludeQuery?: boolean
  stateCodes?: string[]
  excludeStates?: boolean
  cnaes?: string[]
  excludeCnaes?: boolean
  legalNatureCodes?: string[]
  excludeLegalNatureCodes?: boolean
  companySizeCodes?: string[]
  excludeCompanySizes?: boolean
  registrationStatuses?: string[]
  excludeRegistrationStatuses?: boolean
  headOfficeOnly?: boolean
  excludeHeadOfficeOnly?: boolean
  shareCapitalMin?: number
  shareCapitalMax?: number
  excludeShareCapitalRange?: boolean
  page?: number
  pageSize?: number
}

export interface ListCompaniesParams {
  stateCodes?: string[]
  excludeStates?: boolean
  cnaes?: string[]
  excludeCnaes?: boolean
  legalNatureCodes?: string[]
  excludeLegalNatureCodes?: boolean
  headOfficeOnly?: boolean
  excludeHeadOfficeOnly?: boolean
  page?: number
  pageSize?: number
}

export interface ListHoldingsParams {
  stateCode?: string
  page?: number
  pageSize?: number
}

export interface CompanyExportStats {
  requestedLimit: number
  exportedCount: number
  scannedCount: number
  withEmailCount: number
  withoutEmailCount: number
  uniqueEmailCount: number
  duplicateEmailSkippedCount: number
}

export interface CompanyExportResult {
  columns: string[]
  stats: CompanyExportStats
  items: Record<string, string | null>[]
}

export interface ExportCompaniesOptions {
  limit?: number
  deduplicateEmail?: boolean
}

export type ExportSearchCompaniesParams = SearchCompaniesParams & ExportCompaniesOptions
export type ExportListCompaniesParams = ListCompaniesParams & ExportCompaniesOptions
