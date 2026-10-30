import type { RouteObject } from 'react-router'
import { MainLayout } from '@/app/layouts/main-layout/main-layout'
import { FullPageLoader } from '@/shared/components/full-page-loader'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'
import { buildRouteObjects } from './build-route-objects'
import { NotFoundPage } from './not-found-page'
import { RequireSession } from './require-session'
import { RouteErrorPage } from './route-error-page'
import { appRoutes } from './route-config'

/** Cây route đầy đủ của app (tách khỏi router.ts để test dùng với createMemoryRouter). */
export const routeObjects: RouteObject[] = [
  {
    path: ROUTE_PATHS.login,
    errorElement: <RouteErrorPage />,
    hydrateFallbackElement: <FullPageLoader />,
    lazy: async () => ({ Component: (await import('@/features/auth/pages/login-page')).default })
  },
  {
    path: ROUTE_PATHS.selectCompany,
    errorElement: <RouteErrorPage />,
    hydrateFallbackElement: <FullPageLoader />,
    lazy: async () => ({ Component: (await import('@/features/auth/pages/select-company-page')).default })
  },
  {
    element: <RequireSession />,
    errorElement: <RouteErrorPage />,
    hydrateFallbackElement: <FullPageLoader />,
    children: [
      {
        path: '/',
        element: <MainLayout />,
        children: [...buildRouteObjects(appRoutes), { path: '*', element: <NotFoundPage /> }]
      }
    ]
  }
]
