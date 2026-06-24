import type { PropsWithChildren } from 'react'
import { FormControlLabel, Stack, Switch, Typography } from '@mui/material'

interface FilterFieldProps extends PropsWithChildren {
  helperText?: string
  exclude: boolean
  onExcludeChange: (exclude: boolean) => void
  showExclude?: boolean
}

export function FilterField({
  helperText,
  exclude,
  onExcludeChange,
  showExclude = true,
  children,
}: FilterFieldProps) {
  const resolvedHelperText = exclude && helperText
    ? `${helperText} Excluindo os valores selecionados.`
    : helperText

  return (
    <Stack spacing={0.5}>
      {children}
      <Stack
        direction="row"
        sx={{ alignItems: 'center', justifyContent: 'space-between', minHeight: 24 }}
      >
        {resolvedHelperText ? (
          <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.4 }}>
            {resolvedHelperText}
          </Typography>
        ) : (
          <span />
        )}
        {showExclude ? (
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={exclude}
                onChange={(_event, checked) => onExcludeChange(checked)}
              />
            }
            label="Exceto"
            labelPlacement="start"
            sx={{ mr: 0, ml: 1, '& .MuiFormControlLabel-label': { fontSize: '0.75rem' } }}
          />
        ) : null}
      </Stack>
    </Stack>
  )
}
