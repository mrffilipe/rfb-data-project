export interface CorporateParticipation {
  controlledCnpjBase: string
  controllingCnpj: string
  controllingLegalName?: string | null
  partnerQualificationCode?: string | null
  entryDate?: string | null
  controlledLegalName?: string | null
}

export interface ListCorporateParticipationsParams {
  controllingCnpj?: string
  controlledCnpj?: string
  page?: number
  pageSize?: number
}
