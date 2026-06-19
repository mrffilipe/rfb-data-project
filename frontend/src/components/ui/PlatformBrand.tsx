import { Box, Typography, type SxProps, type Theme } from '@mui/material'
import { Link } from 'react-router'
import { PlatformLogo } from './PlatformLogo'

interface PlatformBrandProps {
  logoSize?: number
  showTitle?: boolean
  to?: string
  sx?: SxProps<Theme>
}

export function PlatformBrand({ logoSize = 40, showTitle = true, to = '/', sx }: PlatformBrandProps) {
  const inner = showTitle ? (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25 }}>
      <PlatformLogo size={logoSize} />
      <Typography variant="h6" component="span" sx={{ fontWeight: 700, letterSpacing: '-0.02em', color: 'primary.main' }}>
        RFB Data
      </Typography>
    </Box>
  ) : (
    <PlatformLogo size={logoSize} sx={{ mx: 0 }} />
  )

  const boxSx = [
    {
      display: 'inline-flex',
      alignItems: 'center',
      textDecoration: 'none',
      color: 'inherit',
    },
    ...(Array.isArray(sx) ? sx : sx ? [sx] : []),
  ] as const

  if (to) {
    return (
      <Box component={Link} to={to} sx={boxSx}>
        {inner}
      </Box>
    )
  }

  return <Box sx={boxSx}>{inner}</Box>
}
