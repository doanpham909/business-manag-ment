import { useQuery } from '@tanstack/react-query'
import type { PagedResult } from './api-envelope'
import { apiClient } from './http-client'

export interface SelectOption {
  value: string
  label: string
}

interface CatalogItemLite {
  code: string
  name: string
}

/** Lựa chọn từ danh mục dùng chung (giá trị = mã mục), chỉ gồm mục đang sử dụng. */
export function useCatalogOptions(type: string): SelectOption[] {
  const { data } = useQuery({
    queryKey: ['admin', 'catalogs', type, 'options'],
    queryFn: () =>
      apiClient.get<PagedResult<CatalogItemLite>>('/v1/admin/catalogs', {
        params: { type, activeOnly: true, pageIndex: 1, pageSize: 100 }
      })
  })
  return (data?.items ?? []).map((item) => ({ value: item.code, label: item.name }))
}

interface DepartmentLite {
  id: string
  code: string
  name: string
}

/** Lựa chọn đơn vị đang hoạt động của công ty hiện hành. */
export function useDepartmentOptions(): SelectOption[] {
  const { data } = useQuery({
    queryKey: ['hrm', 'departments', 'options'],
    queryFn: () => apiClient.get<DepartmentLite[]>('/v1/hrm/departments', { params: { activeOnly: true } })
  })
  return (data ?? []).map((d) => ({ value: d.id, label: `${d.code} - ${d.name}` }))
}
