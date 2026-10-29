import { ApiError } from './api-error'

/** Lỗi nghiệp vụ dạng { code, message } — khớp `Error` record của backend (BuildingBlocks.Shared.Results). */
export interface ApiErrorItem {
  code: string
  message: string
}

/**
 * Envelope thành công mà backend trả về (serialize từ `Result<T>`, giống QMS):
 * `{ isSuccess, isFailure, error, warningMessage, value }`.
 */
export interface ApiEnvelope<T> {
  isSuccess: boolean
  isFailure: boolean
  error: ApiErrorItem
  warningMessage?: string | null
  value?: T | null
}

/** Trang dữ liệu phân trang — khớp `PagedResult<T>` của backend (pageIndex bắt đầu từ 1). */
export interface PagedResult<T> {
  items: T[]
  pageIndex: number
  pageSize: number
  totalCount: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

/** Nhận diện body có phải envelope `Result<T>` hay không (vd /health trả JSON thường). */
export function isApiEnvelope(body: unknown): body is ApiEnvelope<unknown> {
  return typeof body === 'object' && body !== null && typeof (body as ApiEnvelope<unknown>).isSuccess === 'boolean'
}

/**
 * Bóc envelope: thành công → trả `value`; thất bại → ném ApiError.
 * Body không phải envelope được trả nguyên vẹn để vẫn dùng được với endpoint ngoài chuẩn.
 */
export function unwrapEnvelope<T>(body: unknown, status = 200): T {
  if (!isApiEnvelope(body)) return body as T
  if (body.isSuccess) return body.value as T
  throw new ApiError({
    status,
    code: body.error?.code || 'UNKNOWN',
    message: body.error?.message || 'Yêu cầu không thành công.'
  })
}
