import { apiClient } from '@/shared/api/http-client'

export interface Department {
  id: string
  code: string
  name: string
  parentId?: string | null
  typeCode?: string | null
  sortOrder: number
  isActive: boolean
  version: number
}

export interface DepartmentInput {
  name: string
  parentId?: string | null
  typeCode?: string | null
  sortOrder: number
}

export interface DepartmentTreeNode extends Department {
  children?: DepartmentTreeNode[]
}

export const departmentKeys = { all: ['hrm', 'departments'] as const }

export const departmentApi = {
  list: () => apiClient.get<Department[]>('/v1/hrm/departments'),
  create: (body: DepartmentInput & { code: string }) => apiClient.post<string>('/v1/hrm/departments', body),
  update: (id: string, body: DepartmentInput & { version: number }) =>
    apiClient.put<void>(`/v1/hrm/departments/${id}`, body),
  setActive: (id: string, isActive: boolean) => apiClient.post<void>(`/v1/hrm/departments/${id}/active`, { isActive }),
  remove: (id: string) => apiClient.delete<void>(`/v1/hrm/departments/${id}`)
}

/** Dựng cây từ danh sách phẳng; đơn vị có cha không còn trong danh sách được đưa lên gốc. */
export function buildDepartmentTree(items: Department[]): DepartmentTreeNode[] {
  const nodes = new Map<string, DepartmentTreeNode>(items.map((d) => [d.id, { ...d }]))
  const roots: DepartmentTreeNode[] = []
  for (const node of nodes.values()) {
    const parent = node.parentId ? nodes.get(node.parentId) : undefined
    if (parent) (parent.children ??= []).push(node)
    else roots.push(node)
  }
  return roots
}
