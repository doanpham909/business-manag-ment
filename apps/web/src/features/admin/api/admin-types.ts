export type CompanyStatus = 'Active' | 'Inactive'

export interface CompanyInput {
  name: string
  taxCode?: string | null
  address?: string | null
  phone?: string | null
}

export interface Company extends CompanyInput {
  id: string
  code: string
  status: CompanyStatus
  version: number
}

export type UserStatus = 'Active' | 'Locked'

export interface UserAssignment {
  companyId: string
  roleIds: string[]
}

export interface UserInput {
  fullName: string
  email?: string | null
  phone?: string | null
  assignments: UserAssignment[]
}

export interface UserListItem {
  id: string
  username: string
  fullName: string
  email?: string | null
  phone?: string | null
  status: UserStatus
  companyCount: number
}

export interface UserDetail extends UserInput {
  id: string
  username: string
  status: UserStatus
  version: number
}

export type DataScope = 'Own' | 'Project' | 'Department' | 'DepartmentTree' | 'All'

export interface RoleDataScope {
  module: string
  scope: DataScope
}

export interface RoleInput {
  name: string
  description?: string | null
  permissions: string[]
  dataScopes: RoleDataScope[]
}

export interface RoleListItem {
  id: string
  code: string
  name: string
  description?: string | null
  isSystem: boolean
  userCount: number
}

export interface RoleDetail extends RoleInput {
  id: string
  code: string
  isSystem: boolean
  version: number
}

export interface PermissionResource {
  module: string
  code: string
  name: string
  items: { code: string; action: string; actionName: string }[]
}

export interface PermissionCatalog {
  modules: Record<string, string>
  resources: PermissionResource[]
}

export interface CatalogType {
  code: string
  name: string
}

export interface CatalogItem {
  id: string
  type: string
  code: string
  name: string
  description?: string | null
  sortOrder: number
  isActive: boolean
  version: number
}

export interface AuditLog {
  id: string
  companyId?: string | null
  userId?: string | null
  entityType: string
  entityId: string
  action: 'Created' | 'Updated' | 'Deleted'
  changes: string
  occurredAt: string
}
