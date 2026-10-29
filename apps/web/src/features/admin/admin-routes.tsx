import { Settings } from 'lucide-react'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'
import type { AppRoute } from '@/shared/types/app-route'

/** Route của module Quản trị hệ thống — được gom vào app/router/route-config.tsx. */
export const adminRoutes: AppRoute[] = [
  {
    path: ROUTE_PATHS.admin,
    title: 'Quản trị hệ thống',
    icon: <Settings size={18} />,
    page: () => import('./pages/admin-page')
  }
]
