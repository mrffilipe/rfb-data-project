import { createBrowserRouter } from 'react-router'
import { AppLayout } from './components/AppLayout'
import {
  CompanyDetailPage,
  CompanyListPage,
  CompanySearchPage,
  CorporateParticipationsPage,
  HoldingsPage,
  HomePage,
  IngestionPage,
  NotFoundPage,
  PartnersPage,
} from './pages'

export const router = createBrowserRouter([
  {
    path: '/',
    Component: AppLayout,
    children: [
      { index: true, Component: HomePage },
      { path: 'companies/search', Component: CompanySearchPage },
      { path: 'companies/holdings', Component: HoldingsPage },
      { path: 'companies/:cnpj', Component: CompanyDetailPage },
      { path: 'companies', Component: CompanyListPage },
      { path: 'partners', Component: PartnersPage },
      { path: 'participations/corporate', Component: CorporateParticipationsPage },
      { path: 'ingestion', Component: IngestionPage },
      { path: '*', Component: NotFoundPage },
    ],
  },
  {
    path: '*',
    Component: NotFoundPage,
  },
])
