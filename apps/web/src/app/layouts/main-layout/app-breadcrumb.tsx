import { BreadcrumbComponent } from '@systemvci/shared-ui'
import { Link, useMatches } from 'react-router'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'
import type { RouteHandle } from '@/shared/types/app-route'

const hasTitle = (handle: unknown): handle is RouteHandle =>
  typeof handle === 'object' && handle !== null && typeof (handle as RouteHandle).title === 'string'

/** Breadcrumb tự sinh từ `handle.title` của các route đang khớp (khai báo trong route-config). */
export function AppBreadcrumb() {
  const crumbs = useMatches().filter((match) => hasTitle(match.handle))

  const items = [
    { key: 'home', title: <Link to={ROUTE_PATHS.dashboard}>Trang chủ</Link> },
    ...crumbs
      .filter((match) => match.pathname !== ROUTE_PATHS.dashboard)
      .map((match, index, list) => {
        const { title } = match.handle as RouteHandle
        const isLast = index === list.length - 1
        return { key: match.id, title: isLast ? title : <Link to={match.pathname}>{title}</Link> }
      })
  ]

  return <BreadcrumbComponent items={items} />
}
