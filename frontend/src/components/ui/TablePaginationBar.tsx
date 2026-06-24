import { Pagination, Stack, Typography } from '@mui/material'

interface TablePaginationBarProps {
  page: number
  pageSize: number
  total: number
  totalIsApproximate?: boolean
  itemsOnPage?: number
  onPageChange: (page: number) => void
  disabled?: boolean
}

export function TablePaginationBar({
  page,
  pageSize,
  total,
  totalIsApproximate = false,
  itemsOnPage,
  onPageChange,
  disabled = false,
}: TablePaginationBarProps) {
  const from = total === 0 ? 0 : (page - 1) * pageSize + 1
  const to = total === 0 ? 0 : from + (itemsOnPage ?? Math.min(pageSize, total - from + 1)) - 1
  const totalLabel = totalIsApproximate ? `${total.toLocaleString('pt-BR')}+` : total.toLocaleString('pt-BR')

  const hasNextPage = totalIsApproximate
    ? (itemsOnPage ?? 0) === pageSize
    : page * pageSize < total

  const count = totalIsApproximate
    ? page + (hasNextPage ? 1 : 0)
    : Math.max(1, Math.ceil(total / pageSize))

  if (total === 0) {
    return null
  }

  return (
    <Stack
      direction={{ xs: 'column', sm: 'row' }}
      spacing={1.5}
      sx={{ alignItems: { sm: 'center' }, justifyContent: 'space-between', pt: 2 }}
    >
      <Typography variant="body2" color="text.secondary">
        {from}–{to} de {totalLabel} resultados
      </Typography>
      <Pagination
        count={Math.max(1, count)}
        page={page}
        onChange={(_event, value) => onPageChange(value)}
        color="primary"
        size="small"
        disabled={disabled}
        showFirstButton={!totalIsApproximate}
        showLastButton={!totalIsApproximate}
      />
    </Stack>
  )
}
