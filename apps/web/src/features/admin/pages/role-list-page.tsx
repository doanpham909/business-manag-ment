import {
  ButtonComponent,
  PrimaryButtonComponent,
  StatusTagComponent,
  type ConfigurableColumn
} from '@systemvci/shared-ui'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Pencil, Plus, Trash2 } from 'lucide-react'
import { useState } from 'react'
import * as Yup from 'yup'
import { usePermission } from '@/shared/auth/auth-store'
import { ConfirmAction } from '@/shared/components/confirm-action'
import { FormDrawer } from '@/shared/components/form-drawer'
import { PageContainer } from '@/shared/components/page-container'
import { ResourceTable } from '@/shared/components/resource-table'
import { adminKeys, roleApi } from '../api/admin-api'
import type { RoleDataScope, RoleListItem } from '../api/admin-types'
import { RolePermissionMatrix } from '../components/role-permission-matrix'

interface RoleFormValues {
  code: string
  name: string
  description: string
  permissions: string[]
  dataScopes: RoleDataScope[]
}

const emptyForm: RoleFormValues = { code: '', name: '', description: '', permissions: [], dataScopes: [] }

const schema = Yup.object({
  code: Yup.string().trim().required('Vui lòng nhập mã vai trò.').max(50, 'Tối đa 50 ký tự.'),
  name: Yup.string().trim().required('Vui lòng nhập tên vai trò.').max(200, 'Tối đa 200 ký tự.'),
  description: Yup.string().nullable().max(1000, 'Tối đa 1000 ký tự.')
})

const columns: ConfigurableColumn<RoleListItem>[] = [
  { title: 'Mã', dataIndex: 'code', width: 180 },
  { title: 'Tên vai trò', dataIndex: 'name', width: 240 },
  { title: 'Mô tả', dataIndex: 'description' },
  { title: 'Số người dùng', dataIndex: 'userCount', width: 130, align: 'right' },
  {
    title: 'Loại',
    dataIndex: 'isSystem',
    width: 120,
    render: (isSystem: boolean) =>
      isSystem ? <StatusTagComponent status='system' label='Hệ thống' colorMap={{ system: 'blue' }} /> : null
  }
]

/** ADM-02, ADM-03: vai trò, quyền và phạm vi dữ liệu. */
export default function RoleListPage() {
  const queryClient = useQueryClient()
  const canCreate = usePermission('admin.role.create')
  const canUpdate = usePermission('admin.role.update')
  const canDelete = usePermission('admin.role.delete')
  const [creating, setCreating] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)

  const detail = useQuery({
    queryKey: [...adminKeys.roles, 'detail', editingId],
    queryFn: () => roleApi.get(editingId!),
    enabled: !!editingId,
    gcTime: 0
  })
  const refresh = () => queryClient.invalidateQueries({ queryKey: adminKeys.roles })

  const editing = editingId ? detail.data : undefined
  const close = () => {
    setCreating(false)
    setEditingId(null)
  }

  return (
    <PageContainer
      title='Vai trò và phân quyền'
      actions={
        canCreate && (
          <PrimaryButtonComponent icon={<Plus size={16} />} onClick={() => setCreating(true)}>
            Thêm vai trò
          </PrimaryButtonComponent>
        )
      }
    >
      <ResourceTable
        queryKey={adminKeys.roles}
        fetchPage={roleApi.list}
        columns={columns}
        actionColumn={{
          width: 180,
          render: (role) => (
            <div className='flex items-center gap-1'>
              <ButtonComponent
                type='link'
                size='small'
                icon={<Pencil size={14} />}
                onClick={() => setEditingId(role.id)}
              >
                {role.isSystem || !canUpdate ? 'Xem' : 'Sửa'}
              </ButtonComponent>
              {canDelete && !role.isSystem && (
                <ConfirmAction
                  label='Xóa'
                  icon={<Trash2 size={14} />}
                  title={`Xóa vai trò ${role.name}?`}
                  danger
                  confirmLabel='Xóa'
                  successMessage='Đã xóa vai trò.'
                  onConfirm={async () => {
                    await roleApi.remove(role.id)
                    await refresh()
                  }}
                />
              )}
            </div>
          )
        }}
      />

      <FormDrawer<RoleFormValues>
        open={creating || (!!editingId && !!editing)}
        width={760}
        title={editing ? `Vai trò ${editing.name}` : 'Thêm vai trò'}
        initialValues={editing ? { ...emptyForm, ...editing, description: editing.description ?? '' } : emptyForm}
        validationSchema={schema}
        fields={[
          { name: 'code', label: 'Mã vai trò', required: true, disabled: !!editing },
          { name: 'name', label: 'Tên vai trò', required: true, disabled: editing?.isSystem },
          { name: 'description', label: 'Mô tả', type: 'textarea', disabled: editing?.isSystem }
        ]}
        onClose={close}
        onSubmit={async ({ code, name, description, permissions, dataScopes }) => {
          const body = { name, description: description || null, permissions, dataScopes }
          if (editing) await roleApi.update(editing.id, { ...body, version: editing.version })
          else await roleApi.create({ ...body, code })
          await refresh()
        }}
      >
        {(form) => (
          <RolePermissionMatrix
            permissions={form.values.permissions}
            dataScopes={form.values.dataScopes}
            disabled={editing?.isSystem}
            onPermissionsChange={(next) => void form.setFieldValue('permissions', next)}
            onDataScopesChange={(next) => void form.setFieldValue('dataScopes', next)}
          />
        )}
      </FormDrawer>
    </PageContainer>
  )
}
