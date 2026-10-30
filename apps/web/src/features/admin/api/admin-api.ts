import type { PagedResult } from '@/shared/api/api-envelope'
import { apiClient } from '@/shared/api/http-client'
import type { ResourceListParams } from '@/shared/components/resource-table'
import type {
  AuditLog,
  CatalogItem,
  CatalogType,
  Company,
  CompanyInput,
  PermissionCatalog,
  RoleDetail,
  RoleInput,
  RoleListItem,
  UserDetail,
  UserInput,
  UserListItem
} from './admin-types'

export const adminKeys = {
  companies: ['admin', 'companies'] as const,
  users: ['admin', 'users'] as const,
  roles: ['admin', 'roles'] as const,
  catalogs: ['admin', 'catalogs'] as const,
  auditLogs: ['admin', 'audit-logs'] as const
}

export const companyApi = {
  list: (params: ResourceListParams) => apiClient.get<PagedResult<Company>>('/v1/admin/companies', { params }),
  create: (body: CompanyInput & { code: string }) => apiClient.post<string>('/v1/admin/companies', body),
  update: (id: string, body: CompanyInput & { version: number }) =>
    apiClient.put<void>(`/v1/admin/companies/${id}`, body),
  setActive: (id: string, isActive: boolean) => apiClient.post<void>(`/v1/admin/companies/${id}/active`, { isActive })
}

export const userApi = {
  list: (params: ResourceListParams) => apiClient.get<PagedResult<UserListItem>>('/v1/admin/users', { params }),
  get: (id: string) => apiClient.get<UserDetail>(`/v1/admin/users/${id}`),
  create: (body: UserInput & { username: string; password: string }) => apiClient.post<string>('/v1/admin/users', body),
  update: (id: string, body: UserInput & { version: number }) => apiClient.put<void>(`/v1/admin/users/${id}`, body),
  lock: (id: string) => apiClient.post<void>(`/v1/admin/users/${id}/lock`),
  unlock: (id: string) => apiClient.post<void>(`/v1/admin/users/${id}/unlock`),
  resetPassword: (id: string, newPassword: string) =>
    apiClient.post<void>(`/v1/admin/users/${id}/reset-password`, { newPassword })
}

export const roleApi = {
  list: (params: ResourceListParams) => apiClient.get<PagedResult<RoleListItem>>('/v1/admin/roles', { params }),
  get: (id: string) => apiClient.get<RoleDetail>(`/v1/admin/roles/${id}`),
  permissionCatalog: () => apiClient.get<PermissionCatalog>('/v1/admin/roles/permission-catalog'),
  create: (body: RoleInput & { code: string }) => apiClient.post<string>('/v1/admin/roles', body),
  update: (id: string, body: RoleInput & { version: number }) => apiClient.put<void>(`/v1/admin/roles/${id}`, body),
  remove: (id: string) => apiClient.delete<void>(`/v1/admin/roles/${id}`)
}

export const catalogApi = {
  types: () => apiClient.get<CatalogType[]>('/v1/admin/catalogs/types'),
  list: (type: string, params: ResourceListParams) =>
    apiClient.get<PagedResult<CatalogItem>>('/v1/admin/catalogs', { params: { ...params, type } }),
  create: (body: { type: string; code: string; name: string; description?: string | null; sortOrder: number }) =>
    apiClient.post<string>('/v1/admin/catalogs', body),
  update: (id: string, body: { name: string; description?: string | null; sortOrder: number; version: number }) =>
    apiClient.put<void>(`/v1/admin/catalogs/${id}`, body),
  setActive: (id: string, isActive: boolean) => apiClient.post<void>(`/v1/admin/catalogs/${id}/active`, { isActive })
}

export const auditLogApi = {
  list: (params: ResourceListParams) =>
    apiClient.get<PagedResult<AuditLog>>('/v1/admin/audit-logs', {
      // Ô tìm kiếm của nhật ký lọc theo loại dữ liệu
      params: { pageIndex: params.pageIndex, pageSize: params.pageSize, entityType: params.searchTerm }
    })
}
