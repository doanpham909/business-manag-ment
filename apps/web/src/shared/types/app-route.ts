import type { ComponentType, ReactNode } from 'react'

/** Module trang được lazy-load: file page phải `export default` component. */
export type LazyPage = () => Promise<{ default: ComponentType }>

/**
 * Một mục trong cấu hình route dùng chung — vừa sinh route của router, vừa sinh menu sidebar + breadcrumb.
 * - Mục có `page` → một trang. Mục có `children` → nhóm menu (submenu), có thể không có `page`.
 * - `hideInMenu` để đăng ký route (vd trang chi tiết) mà không hiện trên sidebar.
 */
export interface AppRoute {
  /** Đường dẫn tuyệt đối, lấy từ ROUTE_PATHS */
  path: string
  /** Nhãn tiếng Việt hiển thị trên menu và breadcrumb */
  title: string
  icon?: ReactNode
  page?: LazyPage
  children?: AppRoute[]
  hideInMenu?: boolean
}

/** Dữ liệu gắn vào `handle` của route để breadcrumb đọc qua useMatches(). */
export interface RouteHandle {
  title: string
}
