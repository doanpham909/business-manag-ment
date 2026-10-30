import { apiClient } from '@/shared/api/http-client'
import type { AuthResponse, SessionInfo } from './auth-types'

/** API xác thực (CMN-01). Refresh token do trình duyệt tự gửi qua cookie HttpOnly. */
export const authApi = {
  login: (body: { username: string; password: string }) => apiClient.post<AuthResponse>('/v1/auth/login', body),
  selectCompany: (body: { companyId?: string | null; allCompanies: boolean }) =>
    apiClient.post<AuthResponse>('/v1/auth/select-company', body),
  refresh: () => apiClient.post<AuthResponse>('/v1/auth/refresh', undefined, { skipAuthRefresh: true }),
  logout: () => apiClient.post<void>('/v1/auth/logout', undefined, { skipAuthRefresh: true }),
  session: () => apiClient.get<SessionInfo>('/v1/auth/session')
}
