import { useQuery } from '@tanstack/react-query'
import { getSystemInfo } from './system-info-api'

/** Query key của feature dashboard — dùng khi cần invalidate/refetch chính xác. */
export const dashboardQueryKeys = {
  all: ['dashboard'] as const,
  systemInfo: () => [...dashboardQueryKeys.all, 'system-info'] as const
}

/**
 * Hook mẫu cho pattern lấy dữ liệu: component chỉ dùng hook này (không gọi axios/useState cho server state).
 * TanStack Query tự truyền AbortSignal để huỷ request khi component unmount.
 */
export function useSystemInfoQuery() {
  return useQuery({
    queryKey: dashboardQueryKeys.systemInfo(),
    queryFn: ({ signal }) => getSystemInfo(signal)
  })
}
