import { ButtonComponent } from '@systemvci/shared-ui'
import { Result } from 'antd'
import { useNavigate } from 'react-router'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'

/** Trang 404 — hiển thị bên trong MainLayout để người dùng vẫn dùng được menu. */
export function NotFoundPage() {
  const navigate = useNavigate()
  return (
    <Result
      status='404'
      title='404'
      subTitle='Trang bạn tìm không tồn tại hoặc đã bị di chuyển.'
      extra={<ButtonComponent onClick={() => navigate(ROUTE_PATHS.dashboard)}>Về trang Tổng quan</ButtonComponent>}
    />
  )
}
