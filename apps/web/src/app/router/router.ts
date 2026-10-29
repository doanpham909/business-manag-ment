import { createBrowserRouter } from 'react-router'
import { routeObjects } from './route-objects'

/** Router của trình duyệt (library mode, data router) — dùng trong app.tsx. */
export const router = createBrowserRouter(routeObjects)
