import { create } from 'zustand'
import type { AuthResponse, SessionInfo } from './auth-types'

interface AuthState {
  /** Access token chỉ giữ trong bộ nhớ (không localStorage) — tải lại trang thì khôi phục bằng refresh cookie */
  accessToken: string | null
  session: SessionInfo | null
  /** Đã thử khôi phục phiên khi mở app hay chưa */
  initialized: boolean
  setAuth: (response: AuthResponse) => void
  clear: () => void
  markInitialized: () => void
}

/** Trạng thái đăng nhập phía client. */
export const useAuthStore = create<AuthState>()((set) => ({
  accessToken: null,
  session: null,
  initialized: false,
  setAuth: ({ accessToken, session }) => set({ accessToken, session, initialized: true }),
  clear: () => set({ accessToken: null, session: null }),
  markInitialized: () => set({ initialized: true })
}))

/** Đã chọn công ty cụ thể hoặc đang ở chế độ Tất cả công ty. */
export const hasWorkingCompany = (session: SessionInfo | null) =>
  !!session && (session.allCompanies || !!session.currentCompany)

/** Kiểm tra quyền ở FE — chỉ để ẩn/hiện; backend mới là nơi kiểm tra thật. */
export const hasPermission = (session: SessionInfo | null, permission?: string) =>
  !permission || !!session?.permissions.includes(permission)

/** Hook tiện dụng cho component. */
export function usePermission(permission?: string) {
  const session = useAuthStore((state) => state.session)
  return hasPermission(session, permission)
}
