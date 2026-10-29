import { accountingRoutes } from '@/features/accounting/accounting-routes'
import { adminRoutes } from '@/features/admin/admin-routes'
import { dashboardRoutes } from '@/features/dashboard/dashboard-routes'
import { inventoryRoutes } from '@/features/inventory/inventory-routes'
import { projectsRoutes } from '@/features/projects/projects-routes'
import type { AppRoute } from '@/shared/types/app-route'

/**
 * NGUỒN DUY NHẤT của route trong app: router, menu sidebar và breadcrumb đều sinh từ mảng này.
 * Thêm module mới = tạo `features/<module>/<module>-routes.tsx` rồi thêm vào đây (thứ tự = thứ tự menu).
 */
export const appRoutes: AppRoute[] = [
  ...dashboardRoutes,
  ...adminRoutes,
  ...inventoryRoutes,
  ...accountingRoutes,
  ...projectsRoutes
]
