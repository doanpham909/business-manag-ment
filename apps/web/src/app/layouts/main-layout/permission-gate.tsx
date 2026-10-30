import { EmptyStateComponent } from '@systemvci/shared-ui'
import type { ReactNode } from 'react'
import { useMatches } from 'react-router'
import { hasPermission, useAuthStore } from '@/shared/auth/auth-store'
import type { RouteHandle } from '@/shared/types/app-route'

/** Chặn hiển thị trang khi người dùng không có quyền của route (backend vẫn trả 403 nếu gọi thẳng API). */
export function PermissionGate({ children }: { children: ReactNode }) {
  const session = useAuthStore((state) => state.session)
  const denied = useMatches().some((match) => {
    const handle = match.handle as RouteHandle | undefined
    return !!handle?.permission && !hasPermission(session, handle.permission)
  })

  if (denied) return <EmptyStateComponent description='Bạn không có quyền truy cập trang này.' />
  return children
}
