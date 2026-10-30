import { Menu } from 'antd'
import { useMemo } from 'react'
import { useLocation, useNavigate } from 'react-router'
import { hasPermission, useAuthStore } from '@/shared/auth/auth-store'
import type { AppRoute } from '@/shared/types/app-route'
import { buildMenuItems, findActiveMenuPath } from './menu-utils'

interface SidebarMenuProps {
  routes: AppRoute[]
  collapsed: boolean
}

/** Menu điều hướng bên trái, sinh hoàn toàn từ route-config. */
export function SidebarMenu({ routes, collapsed }: SidebarMenuProps) {
  const navigate = useNavigate()
  const { pathname } = useLocation()

  const session = useAuthStore((state) => state.session)
  const items = useMemo(
    () => buildMenuItems(routes, (permission) => hasPermission(session, permission)),
    [routes, session]
  )
  const activePath = findActiveMenuPath(routes, pathname)
  const selectedKey = activePath.at(-1)

  return (
    <Menu
      mode='inline'
      items={items}
      selectedKeys={selectedKey ? [selectedKey] : []}
      // Khi thu gọn, antd tự hiển thị submenu dạng popup → không ép mở
      defaultOpenKeys={collapsed ? undefined : activePath.slice(0, -1)}
      onClick={({ key }) => navigate(key)}
      className='border-e-0'
    />
  )
}
