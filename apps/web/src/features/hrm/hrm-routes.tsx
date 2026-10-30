import { Users } from 'lucide-react'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'
import type { AppRoute } from '@/shared/types/app-route'

/** Route của module Tổ chức – Nhân sự — được gom vào app/router/route-config.ts. */
export const hrmRoutes: AppRoute[] = [
  {
    path: ROUTE_PATHS.hrm,
    title: 'Tổ chức – Nhân sự',
    icon: <Users size={18} />,
    children: [
      {
        path: ROUTE_PATHS.hrmDepartments,
        title: 'Cơ cấu tổ chức',
        permission: 'hrm.department.view',
        page: () => import('./pages/department-page')
      }
    ]
  }
]
