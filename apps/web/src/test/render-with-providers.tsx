import { render } from '@testing-library/react'
// RouterProvider lấy từ 'react-router' (không phải 'react-router/dom'): trong Node, 'react-router/dom'
// nạp một bản react-router khác → context router không khớp (dual package).
import { createMemoryRouter, RouterProvider, type RouteObject } from 'react-router'
import { AppProviders } from '@/app/providers/app-providers'
import { createQueryClient } from '@/shared/api/query-client'

/** Render cây route trong memory router + toàn bộ provider của app (QueryClient riêng cho mỗi test). */
export function renderRoutes(routes: RouteObject[], initialPath = '/') {
  const router = createMemoryRouter(routes, { initialEntries: [initialPath] })
  const queryClient = createQueryClient()
  queryClient.setDefaultOptions({ queries: { retry: false } })

  const utils = render(
    <AppProviders queryClient={queryClient}>
      <RouterProvider router={router} />
    </AppProviders>
  )
  return { ...utils, router, queryClient }
}
