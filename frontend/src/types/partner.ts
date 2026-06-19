export interface Partner {
  cnpjBase: string
  partnerName: string
  partnerTypeIdentifier?: string | null
  partnerDocument?: string | null
  partnerQualificationCode?: string | null
  entryDate?: string | null
}

export interface GetCompaniesByPartnerParams {
  document: string
  page?: number
  pageSize?: number
}

export interface GetPartnersByCnpjParams {
  cnpj: string
  page?: number
  pageSize?: number
}
