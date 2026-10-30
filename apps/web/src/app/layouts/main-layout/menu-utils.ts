import type { MenuProps } from 'antd'
import type { AppRoute } from '@/shared/types/app-route'

type MenuItem = Required<MenuProps>['items'][number]

/** Sinh item cho antd Menu từ cấu hình route; key = path để điều hướng trực tiếp khi click. */
export function buildMenuItems(routes: AppRoute[], canSee: (permission?: string) => boolean = () => true): MenuItem[] {
  return routes
    .filter((route) => !route.hideInMenu && canSee(route.permission))
    .map((route) => {
      const children = route.children ? buildMenuItems(route.children, canSee) : []
      // Nhóm không có trang riêng và không còn mục con nào được thấy → ẩn cả nhóm
      if (route.children?.length && !children.length && !route.page) return null
      return children.length
        ? { key: route.path, icon: route.icon, label: route.title, children }
        : { key: route.path, icon: route.icon, label: route.title }
    })
    .filter((item) => item !== null)
}

/** Path `routePath` có "chứa" URL hiện tại không ('/' chỉ khớp chính xác để không khớp mọi trang). */
const matchesPath = (routePath: string, pathname: string) =>
  routePath === '/' ? pathname === '/' : pathname === routePath || pathname.startsWith(`${routePath}/`)

/**
 * Tìm chuỗi path từ gốc tới mục menu khớp sâu nhất với URL.
 * Phần tử cuối là mục được chọn; các phần tử trước là submenu cần mở.
 */
export function findActiveMenuPath(routes: AppRoute[], pathname: string): string[] {
  for (const route of routes) {
    if (route.hideInMenu || !matchesPath(route.path, pathname)) continue
    const childPath = route.children ? findActiveMenuPath(route.children, pathname) : []
    return [route.path, ...childPath]
  }
  return []
}
