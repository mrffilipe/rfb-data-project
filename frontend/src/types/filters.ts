import type { LookupItem } from './lookup'

export interface LookupFilterSelection {
  values: LookupItem[]
  exclude: boolean
}

export interface TextFilterSelection {
  value: string
  exclude: boolean
}

export interface RangeFilterSelection {
  min: string
  max: string
  exclude: boolean
}

export interface BooleanFilterSelection {
  value: boolean
  exclude: boolean
}

export function emptyLookupFilter(): LookupFilterSelection {
  return { values: [], exclude: false }
}

export function emptyTextFilter(): TextFilterSelection {
  return { value: '', exclude: false }
}

export function emptyRangeFilter(): RangeFilterSelection {
  return { min: '', max: '', exclude: false }
}

export function emptyBooleanFilter(): BooleanFilterSelection {
  return { value: false, exclude: false }
}

export function lookupCodes(filter: LookupFilterSelection): string[] | undefined {
  const codes = filter.values.map((item) => item.code)
  return codes.length > 0 ? codes : undefined
}
