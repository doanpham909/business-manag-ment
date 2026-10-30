import { MultiSelectComponent, SkeletonFormComponent, SwitchComponent, TypographyComponent } from '@systemvci/shared-ui'
import { useQuery } from '@tanstack/react-query'
import { adminKeys, companyApi, roleApi } from '../api/admin-api'
import type { UserAssignment } from '../api/admin-types'

interface UserAssignmentsFieldProps {
  value: UserAssignment[]
  onChange: (next: UserAssignment[]) => void
}

const PAGE = { pageIndex: 1, pageSize: 100 }

/** Chọn công ty người dùng được vào và vai trò tại từng công ty (vai trò gán theo công ty — ADR-0002). */
export function UserAssignmentsField({ value, onChange }: UserAssignmentsFieldProps) {
  const companies = useQuery({ queryKey: [...adminKeys.companies, 'options'], queryFn: () => companyApi.list(PAGE) })
  const roles = useQuery({ queryKey: [...adminKeys.roles, 'options'], queryFn: () => roleApi.list(PAGE) })

  if (companies.isPending || roles.isPending) return <SkeletonFormComponent fields={2} />

  const roleOptions = (roles.data?.items ?? []).map((r) => ({ value: r.id, label: r.name }))

  const toggle = (companyId: string, enabled: boolean) =>
    onChange(enabled ? [...value, { companyId, roleIds: [] }] : value.filter((a) => a.companyId !== companyId))

  const setRoles = (companyId: string, roleIds: string[]) =>
    onChange(value.map((a) => (a.companyId === companyId ? { ...a, roleIds } : a)))

  return (
    <div className='flex flex-col gap-3'>
      <TypographyComponent variant='body-medium' as='span'>
        Công ty được vào và vai trò
      </TypographyComponent>
      {(companies.data?.items ?? []).map((company) => {
        const assignment = value.find((a) => a.companyId === company.id)
        return (
          <div key={company.id} className='flex flex-col gap-2 rounded-lg border border-solid border-divider p-3'>
            <div className='flex items-center justify-between gap-2'>
              <TypographyComponent variant='body' as='span'>
                {company.name}
              </TypographyComponent>
              <SwitchComponent
                aria-label={`Cho phép vào ${company.name}`}
                checked={!!assignment}
                onChange={(enabled) => toggle(company.id, enabled)}
              />
            </div>
            {assignment && (
              <MultiSelectComponent
                aria-label={`Vai trò tại ${company.name}`}
                className='w-full'
                placeholder='Chọn vai trò'
                optionFilterProp='label'
                options={roleOptions}
                value={assignment.roleIds}
                onChange={(next) => setRoles(company.id, next as string[])}
              />
            )}
          </div>
        )
      })}
    </div>
  )
}
