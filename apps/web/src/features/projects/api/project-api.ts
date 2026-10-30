import type { PagedResult } from '@/shared/api/api-envelope'
import { apiClient } from '@/shared/api/http-client'
import type { ResourceListParams } from '@/shared/components/resource-table'

export type ProjectStatus = 'InProgress' | 'Completed' | 'Closed'

export interface ProjectInput {
  name: string
  parentProjectId?: string | null
  projectTypeCode?: string | null
  constructionTypeCode?: string | null
  location?: string | null
  departmentId?: string | null
  managerName?: string | null
  startDate?: string | null
  endDate?: string | null
  description?: string | null
}

export interface Project extends ProjectInput {
  id: string
  code: string
  status: ProjectStatus
  version: number
}

export const PROJECT_STATUS_LABELS: Record<ProjectStatus, string> = {
  InProgress: 'Đang thực hiện',
  Completed: 'Hoàn thành',
  Closed: 'Đóng'
}

export const projectKeys = { all: ['projects', 'projects'] as const }

export const projectApi = {
  list: (params: ResourceListParams) => apiClient.get<PagedResult<Project>>('/v1/projects/projects', { params }),
  create: (body: ProjectInput & { code: string }) => apiClient.post<string>('/v1/projects/projects', body),
  update: (id: string, body: ProjectInput & { version: number }) =>
    apiClient.put<void>(`/v1/projects/projects/${id}`, body),
  copy: (id: string, newCode: string) => apiClient.post<string>(`/v1/projects/projects/${id}/copy`, { newCode }),
  changeStatus: (id: string, status: ProjectStatus, version: number) =>
    apiClient.post<void>(`/v1/projects/projects/${id}/status`, { status, version }),
  remove: (id: string) => apiClient.delete<void>(`/v1/projects/projects/${id}`)
}
