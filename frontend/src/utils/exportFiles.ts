import * as XLSX from 'xlsx'
import type { CompanyExportResult } from '../types'

function triggerDownload(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = filename
  anchor.click()
  URL.revokeObjectURL(url)
}

function rowsFromExport(data: CompanyExportResult): string[][] {
  return data.items.map((item) =>
    data.columns.map((column) => item[column] ?? ''),
  )
}

function timestamp(): string {
  const now = new Date()
  const pad = (value: number) => String(value).padStart(2, '0')
  return `${now.getFullYear()}${pad(now.getMonth() + 1)}${pad(now.getDate())}_${pad(now.getHours())}${pad(now.getMinutes())}`
}

export function downloadExportJson(data: CompanyExportResult, basename = 'empresas'): void {
  const payload = {
    columns: data.columns,
    stats: data.stats,
    items: data.items,
  }
  const blob = new Blob([JSON.stringify(payload, null, 2)], { type: 'application/json;charset=utf-8' })
  triggerDownload(blob, `${basename}_${timestamp()}.json`)
}

export function downloadExportCsv(data: CompanyExportResult, basename = 'empresas'): void {
  const escape = (value: string) => {
    if (value.includes('"') || value.includes(',') || value.includes('\n') || value.includes('\r')) {
      return `"${value.replace(/"/g, '""')}"`
    }
    return value
  }

  const lines = [
    data.columns.map(escape).join(','),
    ...rowsFromExport(data).map((row) => row.map((cell) => escape(String(cell))).join(',')),
  ]

  const blob = new Blob([`\uFEFF${lines.join('\r\n')}`], { type: 'text/csv;charset=utf-8' })
  triggerDownload(blob, `${basename}_${timestamp()}.csv`)
}

export function downloadExportXlsx(data: CompanyExportResult, basename = 'empresas'): void {
  const sheetData = [data.columns, ...rowsFromExport(data)]
  const worksheet = XLSX.utils.aoa_to_sheet(sheetData)
  const workbook = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(workbook, worksheet, 'empresas')
  XLSX.writeFile(workbook, `${basename}_${timestamp()}.xlsx`)
}

export function formatExportStats(stats: CompanyExportResult['stats']): string {
  const parts = [
    `${stats.scannedCount} analisados`,
    `${stats.duplicateEmailSkippedCount} duplicados ignorados`,
    `${stats.exportedCount} leads/sócios exportados (com e-mail)`,
  ]

  if (stats.withoutEmailCount > 0) {
    parts.splice(1, 0, `${stats.withoutEmailCount} sem e-mail ignorados`)
  }

  if (stats.exportedCount < stats.requestedLimit) {
    parts.push('menos que o solicitado — universo filtrado esgotado')
  }

  return parts.join(' · ')
}
