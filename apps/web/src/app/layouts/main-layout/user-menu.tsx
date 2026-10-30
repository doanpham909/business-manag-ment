import { ButtonComponent } from '@systemvci/shared-ui'
import { useQueryClient } from '@tanstack/react-query'
import { Dropdown } from 'antd'
import { CircleUserRound, LogOut } from 'lucide-react'
import { useNavigate } from 'react-router'
import { authApi } from '@/shared/auth/auth-api'
import { useAuthStore } from '@/shared/auth/auth-store'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'

/** Tên người dùng + đăng xuất ở góc phải header. */
export function UserMenu() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { session, clear } = useAuthStore()

  const logout = async () => {
    await authApi.logout().catch(() => undefined)
    clear()
    queryClient.clear()
    navigate(ROUTE_PATHS.login, { replace: true })
  }

  return (
    <Dropdown
      trigger={['click']}
      menu={{
        items: [{ key: 'logout', icon: <LogOut size={16} />, label: 'Đăng xuất', onClick: () => void logout() }]
      }}
    >
      <ButtonComponent type='text' icon={<CircleUserRound size={18} />}>
        {session?.user.fullName}
      </ButtonComponent>
    </Dropdown>
  )
}
