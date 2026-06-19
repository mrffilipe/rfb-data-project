import type { PropsWithChildren } from 'react'
import { Stack } from '@mui/material'
import { formSpacing } from '../../theme/tokens'

interface FormActionsProps extends PropsWithChildren {
  alignEnd?: boolean
}

export function FormActions({ children, alignEnd = true }: FormActionsProps) {
  return (
    <Stack
      direction="row"
      spacing={formSpacing.grid}
      sx={{
        pt: formSpacing.actionsTop,
        width: '100%',
        alignItems: 'center',
        justifyContent: alignEnd ? 'flex-end' : 'center',
      }}
    >
      {children}
    </Stack>
  )
}
