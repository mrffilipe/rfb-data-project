export interface LookupItem {
  code: string
  description: string
}

export interface LookupSearchParams {
  query: string
  page?: number
  pageSize?: number
}
