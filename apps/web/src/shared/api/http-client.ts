import axios, { type AxiosRequestConfig } from 'axios'
import { unwrapEnvelope } from './api-envelope'
import { toApiError } from './api-error'

/** Base URL của API; mặc định `/api` (dev đi qua proxy của Vite, prod do reverse proxy phục vụ). */
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'

/**
 * Axios instance dùng chung. Interceptor lỗi quy mọi lỗi về ApiError.
 * Điểm mở rộng sau này: gắn `Authorization` / `X-Tenant-Id` trong request interceptor.
 */
export const http = axios.create({
  baseURL: API_BASE_URL,
  timeout: 20_000,
  headers: { 'Content-Type': 'application/json' }
})

http.interceptors.response.use(
  (response) => response,
  (error: unknown) => Promise.reject(toApiError(error))
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
