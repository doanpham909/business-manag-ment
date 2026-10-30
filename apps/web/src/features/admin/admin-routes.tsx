import { Settings } from 'lucide-react'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'
import type { AppRoute } from '@/shared/types/app-route'

/** Route của module Quản trị hệ thống — được gom vào app/router/route-config.ts. */
export const adminRoutes: AppRoute[] = [
  {
    path: ROUTE_PATHS.admin,
    title: 'Quản trị hệ thống',
    icon: <Settings size={18} />,
    children: [
      {
        path: ROUTE_PATHS.adminCompanies,
        title: 'Công ty',
        permission: 'admin.company.view',
        page: () => import('./pages/company-list-page')
      },
      {
        path: ROUTE_PATHS.adminUsers,
        title: 'Người dùng',
        permission: 'admin.user.view',
        page: () => import('./pages/user-list-page')
      },
      {
        path: ROUTE_PATHS.adminRoles,
        title: 'Vai trò và phân quyền',
        permission: 'admin.role.view',
        page: () => import('./pages/role-list-page')
      },
      {
        path: ROUTE_PATHS.adminCatalogs,
        title: 'Danh mục dùng chung',
        permission: 'admin.catalog.view',
        page: () => import('./pages/catalog-page')
      },
      {
        path: ROUTE_PATHS.adminAuditLogs,
        title: 'Nhật ký thay đổi',
        permission: 'admin.auditlog.view',
        page: () => import('./pages/audit-log-page')
      }
    ]
  }
]
