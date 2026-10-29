import type { RouteObject } from 'react-router'
import { MainLayout } from '@/app/layouts/main-layout/main-layout'
import { FullPageLoader } from '@/shared/components/full-page-loader'
import { buildRouteObjects } from './build-route-objects'
import { NotFoundPage } from './not-found-page'
import { RouteErrorPage } from './route-error-page'
import { appRoutes } from './route-config'

/** Cây route đầy đủ của app (tách khỏi router.ts để test dùng với createMemoryRouter). */
export const routeObjects: RouteObject[] = [
  {
    path: '/',
    element: <MainLayout />,
    errorElement: <RouteErrorPage />,
    hydrateFallbackElement: <FullPageLoader />,
    children: [...buildRouteObjects(appRoutes), { path: '*', element: <NotFoundPage /> }]
  }
]
