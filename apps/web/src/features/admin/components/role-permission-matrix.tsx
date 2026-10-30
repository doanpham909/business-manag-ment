import { SelectComponent, SkeletonFormComponent, TypographyComponent } from '@systemvci/shared-ui'
import { useQuery } from '@tanstack/react-query'
import { Checkbox, Collapse } from 'antd'
import { adminKeys, roleApi } from '../api/admin-api'
import type { DataScope, RoleDataScope } from '../api/admin-types'

const SCOPE_OPTIONS: { value: DataScope; label: string }[] = [
  { value: 'All', label: 'Toàn công ty' },
  { value: 'DepartmentTree', label: 'Đơn vị và cấp dưới' },
  { value: 'Department', label: 'Đúng đơn vị của mình' },
  { value: 'Project', label: 'Dự án được gán' },
  { value: 'Own', label: 'Của mình' }
]

interface RolePermissionMatrixProps {
  permissions: string[]
  dataScopes: RoleDataScope[]
  disabled?: boolean
  onPermissionsChange: (next: string[]) => void
  onDataScopesChange: (next: RoleDataScope[]) => void
}

/** Cấu hình quyền theo tài nguyên và phạm vi dữ liệu theo phân hệ của một vai trò (ADM-02, ADM-03). */
export function RolePermissionMatrix({
  permissions,
  dataScopes,
  disabled,
  onPermissionsChange,
  onDataScopesChange
}: RolePermissionMatrixProps) {
  const catalog = useQuery({
    queryKey: [...adminKeys.roles, 'permission-catalog'],
    queryFn: roleApi.permissionCatalog,
    staleTime: Infinity
  })
  if (!catalog.data) return <SkeletonFormComponent fields={4} />

  const { modules, resources } = catalog.data
  const granted = new Set(permissions)

  const toggle = (code: string, checked: boolean) => {
    const next = new Set(granted)
    if (checked) next.add(code)
    else next.delete(code)
    onPermissionsChange([...next])
  }

  const setScope = (module: string, scope: DataScope) =>
    onDataScopesChange([...dataScopes.filter((s) => s.module !== module), { module, scope }])

  const items = Object.entries(modules).map(([module, name]) => ({
    key: module,
    label: name,
    children: (
      <div className='flex flex-col gap-3'>
        <div className='flex items-center gap-2'>
          <TypographyComponent variant='body-medium' as='span'>
            Phạm vi dữ liệu
          </TypographyComponent>
          <SelectComponent
            aria-label={`Phạm vi dữ liệu ${name}`}
            className='w-64'
            disabled={disabled}
            options={SCOPE_OPTIONS}
            value={dataScopes.find((s) => s.module === module)?.scope ?? 'Own'}
            onChange={(scope) => setScope(module, scope as DataScope)}
          />
        </div>
        {resources
          .filter((resource) => resource.module === module)
          .map((resource) => (
            <div key={resource.code} className='flex flex-col gap-1'>
              <TypographyComponent variant='caption-semibold' as='span'>
                {resource.name}
              </TypographyComponent>
              <div className='flex flex-wrap gap-x-4 gap-y-1'>
                {resource.items.map((item) => (
                  <Checkbox
                    key={item.code}
                    disabled={disabled}
                    checked={granted.has(item.code)}
                    onChange={(event) => toggle(item.code, event.target.checked)}
                  >
                    {item.actionName}
                  </Checkbox>
                ))}
              </div>
            </div>
          ))}
      </div>
    )
  }))

  return <Collapse items={items} defaultActiveKey={['admin']} />
}
