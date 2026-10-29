import { apiClient } from '@/shared/api/http-client'

/** Dữ liệu `value` của GET /api/v1/system/info (bên trong envelope Result<T>). */
export interface SystemInfo {
  name: string
  version: string
  environment: string
  /** Thời gian máy chủ, chuỗi ISO 8601 */
  serverTime: string
}

/** Đường dẫn tương đối so với API_BASE_URL (`/api`). */
export const SYSTEM_INFO_URL = '/v1/system/info'

/** Gọi API lấy thông tin hệ thống — trả về `value` đã bóc envelope. */
export const getSystemInfo = (signal?: AbortSignal) => apiClient.get<SystemInfo>(SYSTEM_INFO_URL, { signal })
