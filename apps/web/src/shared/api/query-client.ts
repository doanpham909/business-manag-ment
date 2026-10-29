import { QueryClient } from '@tanstack/react-query'
import { ApiError } from './api-error'

/** Không retry lỗi phía client (4xx) — retry vô ích; chỉ retry lỗi mạng/5xx tối đa 2 lần. */
function shouldRetry(failureCount: number, error: unknown): boolean {
  if (error instanceof ApiError && error.status >= 400 && error.status < 500) return false
  return failureCount < 2
}

/** Tạo QueryClient với cấu hình mặc định của app (hàm factory để test tạo instance riêng). */
export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        refetchOnWindowFocus: false,
        retry: shouldRetry
      },
      mutations: { retry: false }
    }
  })
}

export const queryClient = createQueryClient()
