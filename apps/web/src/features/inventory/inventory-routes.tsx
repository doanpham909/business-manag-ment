import { Warehouse } from 'lucide-react'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'
import type { AppRoute } from '@/shared/types/app-route'

/** Route của module Kho — được gom vào app/router/route-config.tsx. */
export const inventoryRoutes: AppRoute[] = [
  {
    path: ROUTE_PATHS.inventory,
    title: 'Kho',
    icon: <Warehouse size={18} />,
    page: () => import('./pages/inventory-page')
  }
]
