import type { RouteObject } from 'react-router'
import type { AppRoute, LazyPage, RouteHandle } from '@/shared/types/app-route'

/** Chuyển page lazy (export default) sang dạng `lazy` mà react-router yêu cầu → mỗi trang là 1 chunk riêng. */
const toLazy = (page: LazyPage) => async () => {
  const { default: Component } = await page()
  return { Component }
}

/**
 * Chuyển cấu hình AppRoute thành RouteObject của react-router.
 * - path '/' → index route của layout gốc.
 * - Nhóm có children → route lồng nhau (nếu nhóm có page thì page là index của nhóm).
 */
export function buildRouteObjects(routes: AppRoute[]): RouteObject[] {
  return routes.map((route): RouteObject => {
    const handle: RouteHandle = { title: route.title, permission: route.permission }

    if (route.children?.length) {
      const children = buildRouteObjects(route.children)
      if (route.page) children.unshift({ index: true, lazy: toLazy(route.page) })
      return { path: route.path, handle, children }
    }

    const lazy = route.page ? toLazy(route.page) : undefined
    return route.path === '/' ? { index: true, handle, lazy } : { path: route.path, handle, lazy }
  })
}
