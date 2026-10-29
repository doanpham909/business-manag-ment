import { RouterProvider } from 'react-router/dom'
import { AppProviders } from './providers/app-providers'
import { router } from './router/router'

/** Component gốc của ứng dụng. */
export function App() {
  return (
    <AppProviders>
      <RouterProvider router={router} />
    </AppProviders>
  )
}
