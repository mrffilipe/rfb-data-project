export function formatCnpj(value: string): string {
  const digits = value.replace(/\D/g, '')
  if (digits.length !== 14) {
    return value
  }
  return digits.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')
}

export function formatDocument(value: string): string {
  const digits = value.replace(/\D/g, '')
  if (digits.length === 11) {
    return digits.replace(/^(\d{3})(\d{3})(\d{3})(\d{2})$/, '$1.$2.$3-$4')
  }
  if (digits.length === 14) {
    return formatCnpj(digits)
  }
  return value
}

export function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return '—'
  }
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }
  return date.toLocaleString('pt-BR')
}

export function formatLookupLabel(item: { code: string; description: string }): string {
  return `${item.code} — ${item.description}`
}

export function resolveLookupDescription(
  code: string | null | undefined,
  labels: ReadonlyMap<string, string>,
): string {
  if (!code) {
    return '—'
  }
  return labels.get(code) ?? code
}

export function toLookupLabelMap(items: { code: string; description: string }[]): Map<string, string> {
  return new Map(items.map((item) => [item.code, item.description]))
}
