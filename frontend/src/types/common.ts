export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalIsApproximate?: boolean
}

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  errors?: Record<string, string[]>
}
