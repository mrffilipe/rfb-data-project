import AccountTreeOutlinedIcon from '@mui/icons-material/AccountTreeOutlined'
import BusinessOutlinedIcon from '@mui/icons-material/BusinessOutlined'
import CloudSyncOutlinedIcon from '@mui/icons-material/CloudSyncOutlined'
import DashboardOutlinedIcon from '@mui/icons-material/DashboardOutlined'
import GroupsOutlinedIcon from '@mui/icons-material/GroupsOutlined'
import MenuIcon from '@mui/icons-material/Menu'
import SearchOutlinedIcon from '@mui/icons-material/SearchOutlined'
import ViewListOutlinedIcon from '@mui/icons-material/ViewListOutlined'
import {
  AppBar,
  Box,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Tooltip,
  Typography,
} from '@mui/material'
import type { ReactElement } from 'react'
import { useMemo, useState } from 'react'
import { Link, Outlet, useLocation } from 'react-router'
import { ThemeModeToggle } from './ThemeModeToggle'
import { PlatformBrand } from './ui/PlatformBrand'
import { layout } from '../theme'

const appBarHeight = 64

interface NavItem {
  to: string
  label: string
  icon: ReactElement
}

interface NavGroup {
  label: string
  items: NavItem[]
}

const navGroups: NavGroup[] = [
  {
    label: 'Geral',
    items: [{ to: '/', label: 'Dashboard', icon: <DashboardOutlinedIcon /> }],
  },
  {
    label: 'Empresas',
    items: [
      { to: '/companies/search', label: 'Busca', icon: <SearchOutlinedIcon /> },
      { to: '/companies', label: 'Listagem', icon: <ViewListOutlinedIcon /> },
      { to: '/companies/holdings', label: 'Holdings', icon: <BusinessOutlinedIcon /> },
    ],
  },
  {
    label: 'Sócios',
    items: [{ to: '/partners', label: 'Consulta', icon: <GroupsOutlinedIcon /> }],
  },
  {
    label: 'Participações',
    items: [{ to: '/participations/corporate', label: 'Societárias', icon: <AccountTreeOutlinedIcon /> }],
  },
  {
    label: 'Sistema',
    items: [{ to: '/ingestion', label: 'Ingestão', icon: <CloudSyncOutlinedIcon /> }],
  },
]

function isNavActive(pathname: string, to: string): boolean {
  if (to === '/') {
    return pathname === '/'
  }
  return pathname === to || pathname.startsWith(`${to}/`)
}

export function AppLayout() {
  const location = useLocation()
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const currentPath = useMemo(() => location.pathname, [location.pathname])

  const sidebarHeader = (
    <Box
      sx={{
        px: 2,
        height: appBarHeight,
        flexShrink: 0,
        display: 'flex',
        alignItems: 'center',
        borderBottom: 1,
        borderColor: 'divider',
      }}
    >
      <PlatformBrand logoSize={32} to="/" />
    </Box>
  )

  const navList = (
    <List sx={{ px: 1.5, py: 1, flex: 1, overflowY: 'auto' }}>
      {navGroups.map((group, groupIndex) => (
        <Box key={group.label}>
          {groupIndex > 0 ? <Divider sx={{ my: 1.5 }} /> : null}
          <Typography
            variant="caption"
            color="text.secondary"
            sx={{ px: 1.5, py: 0.5, display: 'block', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.06em' }}
          >
            {group.label}
          </Typography>
          {group.items.map((item) => (
            <ListItemButton
              key={item.to}
              component={Link}
              to={item.to}
              selected={isNavActive(currentPath, item.to)}
              onClick={() => setSidebarOpen(false)}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} slotProps={{ primary: { sx: { fontWeight: 500 } } }} />
            </ListItemButton>
          ))}
        </Box>
      ))}
    </List>
  )

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <Drawer
        variant="temporary"
        open={sidebarOpen}
        onClose={() => setSidebarOpen(false)}
        ModalProps={{ keepMounted: true }}
        sx={{
          zIndex: (theme) => theme.zIndex.modal,
          '& .MuiBackdrop-root': {
            backgroundColor: 'rgba(0, 0, 0, 0.45)',
          },
          '& .MuiDrawer-paper': {
            boxSizing: 'border-box',
            width: layout.sidebarWidth,
            top: 0,
            height: '100vh',
            display: 'flex',
            flexDirection: 'column',
          },
        }}
      >
        {sidebarHeader}
        {navList}
      </Drawer>

      <Box sx={{ display: 'flex', flexDirection: 'column', flexGrow: 1, minWidth: 0, width: '100%' }}>
        <AppBar position="fixed" color="inherit" elevation={0} sx={{ width: '100%', left: 0 }}>
          <Toolbar sx={{ gap: 1, minHeight: appBarHeight, color: 'text.primary' }}>
            <Tooltip title={sidebarOpen ? 'Recolher menu' : 'Abrir menu'}>
              <IconButton
                edge="start"
                color="inherit"
                onClick={() => setSidebarOpen((prev) => !prev)}
                aria-label={sidebarOpen ? 'Recolher menu' : 'Abrir menu'}
              >
                <MenuIcon />
              </IconButton>
            </Tooltip>
            <PlatformBrand logoSize={32} to="/" />
            <Box sx={{ ml: 'auto', display: 'flex', alignItems: 'center', gap: 1 }}>
              <ThemeModeToggle />
            </Box>
          </Toolbar>
        </AppBar>

        <Box
          component="main"
          sx={{
            flexGrow: 1,
            mt: `${appBarHeight}px`,
            px: { xs: 2, sm: 3, lg: 4 },
            py: 3,
          }}
        >
          <Box sx={{ width: '100%', maxWidth: layout.contentMaxWidth, mx: 'auto' }}>
            <Outlet />
          </Box>
        </Box>
      </Box>
    </Box>
  )
}
