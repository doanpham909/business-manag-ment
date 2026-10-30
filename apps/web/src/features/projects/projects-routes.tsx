import { FolderKanban } from 'lucide-react'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'
import type { AppRoute } from '@/shared/types/app-route'

/** Route của module Quản lý dự án — được gom vào app/router/route-config.tsx. */
export const projectsRoutes: AppRoute[] = [
  {
    path: ROUTE_PATHS.projects,
    title: 'Quản lý dự án',
    icon: <FolderKanban size={18} />,
    permission: 'projects.project.view',
    page: () => import('./pages/project-list-page')
  }
]
