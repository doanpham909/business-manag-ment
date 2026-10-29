import { LayoutDashboard } from 'lucide-react'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'
import type { AppRoute } from '@/shared/types/app-route'

/** Route của trang Tổng quan — được gom vào app/router/route-config.tsx. */
export const dashboardRoutes: AppRoute[] = [
  {
    path: ROUTE_PATHS.dashboard,
    title: 'Tổng quan',
    icon: <LayoutDashboard size={18} />,
    page: () => import('./pages/dashboard-page')
  }
]
