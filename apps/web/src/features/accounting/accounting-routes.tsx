import { Calculator } from 'lucide-react'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'
import type { AppRoute } from '@/shared/types/app-route'

/** Route của module Kế toán — được gom vào app/router/route-config.tsx. */
export const accountingRoutes: AppRoute[] = [
  {
    path: ROUTE_PATHS.accounting,
    title: 'Kế toán',
    icon: <Calculator size={18} />,
    page: () => import('./pages/accounting-page')
  }
]
