/** Công ty người dùng được vào. */
export interface CompanyOption {
  id: string
  code: string
  name: string
}

/** Thông tin phiên — khớp `SessionInfo` của backend. */
export interface SessionInfo {
  user: { id: string; username: string; fullName: string }
  companies: CompanyOption[]
  currentCompany: CompanyOption | null
  allCompanies: boolean
  canViewAllCompanies: boolean
  permissions: string[]
}

/** Kết quả đăng nhập / chọn công ty / làm mới phiên (refresh token nằm trong cookie HttpOnly). */
export interface AuthResponse {
  accessToken: string
  expiresAt: string
  session: SessionInfo
}
