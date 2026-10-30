import axios, { isAxiosError, type AxiosRequestConfig, type InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/shared/auth/auth-store'
import type { AuthResponse } from '@/shared/auth/auth-types'
import { unwrapEnvelope } from './api-envelope'
import { toApiError } from './api-error'

declare module 'axios' {
  interface AxiosRequestConfig {
    /** Không tự làm mới phiên khi gặp 401 (chính các request đăng nhập/làm mới/đăng xuất) */
    skipAuthRefresh?: boolean
    /** Đã thử lại sau khi làm mới phiên */
    retriedAfterRefresh?: boolean
  }
}

/** Base URL của API; mặc định `/api` (dev đi qua proxy của Vite, prod do reverse proxy phục vụ). */
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'

/** Axios instance dùng chung: gắn access token, tự làm mới phiên khi 401, quy mọi lỗi về ApiError. */
export const http = axios.create({
  baseURL: API_BASE_URL,
  timeout: 20_000,
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' }
})

http.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

/** Một lần làm mới dùng chung cho mọi request đang chờ (tránh gọi refresh song song → token bị xoay vòng hỏng). */
let refreshing: Promise<boolean> | null = null

export function refreshSession(): Promise<boolean> {
  refreshing ??= http
    .post('/v1/auth/refresh', undefined, { skipAuthRefresh: true })
    .then((response) => {
      useAuthStore.getState().setAuth(unwrapEnvelope<AuthResponse>(response.data, response.status))
      return true
    })
    .catch(() => {
      useAuthStore.getState().clear()
      return false
    })
    .finally(() => {
      refreshing = null
    })
  return refreshing
}

http.interceptors.response.use(
  (response) => response,
  async (error: unknown) => {
    if (isAxiosError(error) && error.response?.status === 401 && error.config) {
      const config = error.config as InternalAxiosRequestConfig
      if (!config.skipAuthRefresh && !config.retriedAfterRefresh && (await refreshSession())) {
        config.retriedAfterRefresh = true
        return http.request(config)
      }
    }
    return Promise.reject(toApiError(error))
  }
)

/** Gửi request và trả thẳng `value` đã bóc envelope (hoặc ném ApiError). */
async function request<T>(config: AxiosRequestConfig): Promise<T> {
  const response = await http.request<unknown>(config)
  return unwrapEnvelope<T>(response.data, response.status)
}

/** API client mỏng: các feature chỉ gọi hàm này, không dùng axios trực tiếp. */
export const apiClient = {
  get: <T>(url: string, config?: AxiosRequestConfig) => request<T>({ ...config, method: 'GET', url }),
  post: <T>(url: string, data?: unknown, config?: AxiosRequestConfig) =>
    request<T>({ ...config, method: 'POST', url, data }),
  put: <T>(url: string, data?: unknown, config?: AxiosRequestConfig) =>
    request<T>({ ...config, method: 'PUT', url, data }),
  delete: <T>(url: string, config?: AxiosRequestConfig) => request<T>({ ...config, method: 'DELETE', url })
}
