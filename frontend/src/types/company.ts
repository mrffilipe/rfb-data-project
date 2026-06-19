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
  stateCode?: string
  cnae?: string
  legalNatureCode?: string
  companySizeCode?: string
  registrationStatus?: string
  headOfficeOnly?: boolean
  shareCapitalMin?: number
  shareCapitalMax?: number
  page?: number
  pageSize?: number
}

export interface ListCompaniesParams {
  stateCode?: string
  cnae?: string
  legalNatureCode?: string
  headOfficeOnly?: boolean
  page?: number
  pageSize?: number
}

export interface ListHoldingsParams {
  stateCode?: string
  page?: number
  pageSize?: number
}
