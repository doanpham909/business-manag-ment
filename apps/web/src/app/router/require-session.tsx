import { useEffect } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router'
import { refreshSession } from '@/shared/api/http-client'
import { hasWorkingCompany, useAuthStore } from '@/shared/auth/auth-store'
import { FullPageLoader } from '@/shared/components/full-page-loader'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'

/**
 * Bảo vệ các trang trong app: khi mở app thử khôi phục phiên bằng refresh cookie;
 * chưa đăng nhập → trang đăng nhập; chưa chọn công ty → trang chọn công ty.
 */
export function RequireSession() {
  const { session, initialized, markInitialized } = useAuthStore()
  const location = useLocation()

  useEffect(() => {
    if (!initialized) void refreshSession().finally(markInitialized)
  }, [initialized, markInitialized])

  if (!initialized) return <FullPageLoader />
  if (!session) return <Navigate to={ROUTE_PATHS.login} replace state={{ from: location.pathname }} />
  if (!hasWorkingCompany(session)) return <Navigate to={ROUTE_PATHS.selectCompany} replace />
  return <Outlet />
}
