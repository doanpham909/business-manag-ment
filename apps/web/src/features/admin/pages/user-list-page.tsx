import {
  ButtonComponent,
  PrimaryButtonComponent,
  StatusTagComponent,
  type ConfigurableColumn
} from '@systemvci/shared-ui'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { KeyRound, Pencil, Plus } from 'lucide-react'
import { useState } from 'react'
import * as Yup from 'yup'
import { usePermission } from '@/shared/auth/auth-store'
import { ConfirmAction } from '@/shared/components/confirm-action'
import { FormDrawer } from '@/shared/components/form-drawer'
import type { FormField } from '@/shared/components/form-field-control'
import { PageContainer } from '@/shared/components/page-container'
import { ResourceTable } from '@/shared/components/resource-table'
import { adminKeys, userApi } from '../api/admin-api'
import type { UserAssignment, UserListItem } from '../api/admin-types'
import { UserAssignmentsField } from '../components/user-assignments-field'

interface UserFormValues {
  username: string
  fullName: string
  email: string
  phone: string
  password: string
  assignments: UserAssignment[]
}

const emptyForm: UserFormValues = { username: '', fullName: '', email: '', phone: '', password: '', assignments: [] }

const baseSchema = {
  fullName: Yup.string().trim().required('Vui lòng nhập họ tên.').max(200, 'Tối đa 200 ký tự.'),
  email: Yup.string().nullable().email('Email không đúng định dạng.').max(200, 'Tối đa 200 ký tự.'),
  phone: Yup.string().nullable().max(20, 'Tối đa 20 ký tự.')
}
const passwordRule = Yup.string().required('Vui lòng nhập mật khẩu.').min(8, 'Mật khẩu tối thiểu 8 ký tự.')
const createSchema = Yup.object({
  ...baseSchema,
  username: Yup.string().trim().required('Vui lòng nhập tên đăng nhập.').max(100, 'Tối đa 100 ký tự.'),
  password: passwordRule
})
const editSchema = Yup.object(baseSchema)
const resetSchema = Yup.object({ password: passwordRule })

const columns: ConfigurableColumn<UserListItem>[] = [
  { title: 'Tên đăng nhập', dataIndex: 'username', width: 180 },
  { title: 'Họ tên', dataIndex: 'fullName' },
  { title: 'Email', dataIndex: 'email' },
  { title: 'Điện thoại', dataIndex: 'phone', width: 140 },
  { title: 'Số công ty', dataIndex: 'companyCount', width: 110, align: 'right' },
  {
    title: 'Trạng thái',
    dataIndex: 'status',
    width: 130,
    render: (status: UserListItem['status']) => (
      <StatusTagComponent
        status={status}
        label={status === 'Active' ? 'Hoạt động' : 'Bị khóa'}
        colorMap={{ Active: 'green', Locked: 'red' }}
      />
    )
  }
]

/** ADM-01: quản lý tài khoản, khóa/mở khóa, đặt lại mật khẩu, gán công ty và vai trò. */
export default function UserListPage() {
  const queryClient = useQueryClient()
  const canCreate = usePermission('admin.user.create')
  const canUpdate = usePermission('admin.user.update')
  const [creating, setCreating] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [resetting, setResetting] = useState<UserListItem | null>(null)

  const detail = useQuery({
    queryKey: [...adminKeys.users, 'detail', editingId],
    queryFn: () => userApi.get(editingId!),
    enabled: !!editingId,
    gcTime: 0
  })
  const refresh = () => queryClient.invalidateQueries({ queryKey: adminKeys.users })

  const editing = editingId ? detail.data : undefined
  const fields: FormField[] = [
    ...(editing ? [] : [{ name: 'username', label: 'Tên đăng nhập', required: true }]),
    { name: 'fullName', label: 'Họ tên', required: true },
    { name: 'email', label: 'Email' },
    { name: 'phone', label: 'Điện thoại' },
    ...(editing ? [] : [{ name: 'password', label: 'Mật khẩu', type: 'password' as const, required: true }])
  ]
  const formOpen = creating || (!!editingId && !!editing)
  const closeForm = () => {
    setCreating(false)
    setEditingId(null)
  }

  return (
    <PageContainer
      title='Người dùng'
      actions={
        canCreate && (
          <PrimaryButtonComponent icon={<Plus size={16} />} onClick={() => setCreating(true)}>
            Thêm người dùng
          </PrimaryButtonComponent>
        )
      }
    >
      <ResourceTable
        queryKey={adminKeys.users}
        fetchPage={userApi.list}
        columns={columns}
        searchPlaceholder='Tìm theo tên đăng nhập, họ tên, email'
        actionColumn={
          canUpdate
            ? {
                width: 280,
                render: (user) => (
                  <div className='flex items-center gap-1'>
                    <ButtonComponent
                      type='link'
                      size='small'
                      icon={<Pencil size={14} />}
                      onClick={() => setEditingId(user.id)}
                    >
                      Sửa
                    </ButtonComponent>
                    <ButtonComponent
                      type='link'
                      size='small'
                      icon={<KeyRound size={14} />}
                      onClick={() => setResetting(user)}
                    >
                      Đặt lại mật khẩu
                    </ButtonComponent>
                    <ConfirmAction
                      label={user.status === 'Active' ? 'Khóa' : 'Mở khóa'}
                      title={`${user.status === 'Active' ? 'Khóa' : 'Mở khóa'} tài khoản ${user.username}?`}
                      content={user.status === 'Active' ? 'Các phiên đang mở của người này sẽ bị chấm dứt.' : undefined}
                      danger={user.status === 'Active'}
                      onConfirm={async () => {
                        await (user.status === 'Active' ? userApi.lock(user.id) : userApi.unlock(user.id))
                        await refresh()
                      }}
                    />
                  </div>
                )
              }
            : undefined
        }
      />

      <FormDrawer<UserFormValues>
        open={formOpen}
        width={640}
        title={editing ? `Sửa người dùng ${editing.username}` : 'Thêm người dùng'}
        initialValues={
          editing ? { ...emptyForm, ...editing, email: editing.email ?? '', phone: editing.phone ?? '' } : emptyForm
        }
        validationSchema={editing ? editSchema : createSchema}
        fields={fields}
        onClose={closeForm}
        onSubmit={async ({ username, password, fullName, email, phone, assignments }) => {
          const body = { fullName, email: email || null, phone: phone || null, assignments }
          if (editing) await userApi.update(editing.id, { ...body, version: editing.version })
          else await userApi.create({ ...body, username, password })
          await refresh()
        }}
      >
        {(form) => (
          <UserAssignmentsField
            value={form.values.assignments}
            onChange={(next) => void form.setFieldValue('assignments', next)}
          />
        )}
      </FormDrawer>

      <FormDrawer
        open={!!resetting}
        title={`Đặt lại mật khẩu cho ${resetting?.username ?? ''}`}
        initialValues={{ password: '' }}
        validationSchema={resetSchema}
        fields={[{ name: 'password', label: 'Mật khẩu mới', type: 'password', required: true }]}
        successMessage='Đã đặt lại mật khẩu. Các phiên đang mở của người này bị chấm dứt.'
        onClose={() => setResetting(null)}
        onSubmit={({ password }) => userApi.resetPassword(resetting!.id, password)}
      />
    </PageContainer>
  )
}
