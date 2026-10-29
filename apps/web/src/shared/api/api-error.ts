import { isAxiosError } from 'axios'
import type { ApiErrorItem } from './api-envelope'

/**
 * Body lỗi thống nhất của backend (`ErrorResponse` — dạng ProblemDetails + code/traceId):
 * `{ code, type, title, status, detail, errors, traceId }`.
 */
export interface ApiErrorResponse {
  code?: string | null
  type?: string
  title?: string
  status?: number
  detail?: string
  errors?: ApiErrorItem[] | null
  traceId?: string | null
}

interface ApiErrorInit {
  status: number
  code: string
  message: string
  details?: ApiErrorItem[]
  traceId?: string
}

/** Lỗi đã chuẩn hóa — mọi lỗi từ http-client đều được quy về kiểu này để UI xử lý đồng nhất. */
export class ApiError extends Error {
  readonly status: number
  readonly code: string
  readonly details: ApiErrorItem[]
  readonly traceId?: string

  constructor({ status, code, message, details = [], traceId }: ApiErrorInit) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
    this.details = details
    this.traceId = traceId
  }
}

/** Thông điệp tiếng Việt mặc định theo HTTP status khi backend không gửi `detail`. */
const DEFAULT_MESSAGES: Record<number, string> = {
  0: 'Không thể kết nối tới máy chủ. Vui lòng kiểm tra mạng hoặc thử lại sau.',
  400: 'Yêu cầu không hợp lệ.',
  401: 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.',
  403: 'Bạn không có quyền thực hiện thao tác này.',
  404: 'Không tìm thấy dữ liệu yêu cầu.',
  422: 'Dữ liệu không hợp lệ.',
  500: 'Máy chủ gặp sự cố. Vui lòng thử lại sau.'
}

const messageFor = (status: number) => DEFAULT_MESSAGES[status] ?? 'Đã xảy ra lỗi không xác định.'

/** Chuẩn hóa mọi lỗi (axios, mạng, timeout, lỗi JS) về ApiError. */
export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error

  if (isAxiosError<ApiErrorResponse>(error)) {
    // Không có response → lỗi mạng / timeout / CORS
    if (!error.response) {
      return new ApiError({ status: 0, code: error.code ?? 'NETWORK_ERROR', message: messageFor(0) })
    }
    const { status, data } = error.response
    const body = typeof data === 'object' && data !== null ? data : {}
    return new ApiError({
      status,
      code: body.code || body.type || `HTTP_${status}`,
      message: body.detail || body.title || messageFor(status),
      details: body.errors ?? [],
      traceId: body.traceId ?? undefined
    })
  }

  const message = error instanceof Error ? error.message : messageFor(-1)
  return new ApiError({ status: -1, code: 'UNKNOWN', message })
}
