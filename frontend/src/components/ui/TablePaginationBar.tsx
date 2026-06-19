import { Pagination, Stack, Typography } from '@mui/material'

interface TablePaginationBarProps {
  page: number
  pageSize: number
  total: number
  onPageChange: (page: number) => void
  disabled?: boolean
}

export function TablePaginationBar({ page, pageSize, total, onPageChange, disabled = false }: TablePaginationBarProps) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize))
  const from = total === 0 ? 0 : (page - 1) * pageSize + 1
  const to = Math.min(page * pageSize, total)

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
        {from}–{to} de {total} resultados
      </Typography>
      <Pagination
        count={totalPages}
        page={page}
        onChange={(_event, value) => onPageChange(value)}
        color="primary"
        size="small"
        disabled={disabled}
        showFirstButton
        showLastButton
      />
    </Stack>
  )
}
