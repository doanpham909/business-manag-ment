import {
  ButtonComponent,
  EmptyStateComponent,
  PrimaryButtonComponent,
  SkeletonTableComponent,
  StatusTagComponent,
  TableComponent,
  type ConfigurableColumn
} from '@systemvci/shared-ui'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Pencil, Plus, Trash2 } from 'lucide-react'
import { useMemo, useState } from 'react'
import * as Yup from 'yup'
import { ApiError } from '@/shared/api/api-error'
import { useCatalogOptions } from '@/shared/api/option-queries'
import { usePermission } from '@/shared/auth/auth-store'
import { ConfirmAction } from '@/shared/components/confirm-action'
import { ErrorState } from '@/shared/components/error-state'
import { FormDrawer } from '@/shared/components/form-drawer'
import { PageContainer } from '@/shared/components/page-container'
import {
  buildDepartmentTree,
  departmentApi,
  departmentKeys,
  type Department,
  type DepartmentTreeNode
} from '../api/department-api'

const schema = Yup.object({
  code: Yup.string().trim().required('Vui lòng nhập mã đơn vị.').max(50, 'Tối đa 50 ký tự.'),
  name: Yup.string().trim().required('Vui lòng nhập tên đơn vị.').max(255, 'Tối đa 255 ký tự.'),
  sortOrder: Yup.number().typeError('Thứ tự phải là số.').min(0, 'Thứ tự không âm.').required('Vui lòng nhập thứ tự.')
})

const emptyForm = { code: '', name: '', parentId: null as string | null, typeCode: null as string | null, sortOrder: 0 }

/** HRM-01: cơ cấu tổ chức dạng cây của công ty đang làm việc. */
export default function DepartmentPage() {
  const queryClient = useQueryClient()
  const canCreate = usePermission('hrm.department.create')
  const canUpdate = usePermission('hrm.department.update')
  const canDelete = usePermission('hrm.department.delete')
  const typeOptions = useCatalogOptions('department-type')
  const query = useQuery({ queryKey: [...departmentKeys.all, 'list'], queryFn: departmentApi.list })
  const [editing, setEditing] = useState<Department | 'new' | null>(null)

  const tree = useMemo(() => buildDepartmentTree(query.data ?? []), [query.data])
  const refresh = () => queryClient.invalidateQueries({ queryKey: departmentKeys.all })
  const current = editing && editing !== 'new' ? editing : null

  const typeLabel = (code?: string | null) => typeOptions.find((o) => o.value === code)?.label ?? code
  const columns: ConfigurableColumn<DepartmentTreeNode>[] = [
    { title: 'Tên đơn vị', dataIndex: 'name' },
    { title: 'Mã', dataIndex: 'code', width: 140 },
    { title: 'Loại', dataIndex: 'typeCode', width: 160, render: (code: string | null) => typeLabel(code) },
    {
      title: 'Trạng thái',
      dataIndex: 'isActive',
      width: 150,
      render: (isActive: boolean) => (
        <StatusTagComponent
          status={isActive ? 'active' : 'inactive'}
          label={isActive ? 'Đang hoạt động' : 'Ngừng hoạt động'}
          colorMap={{ active: 'green', inactive: 'gray' }}
        />
      )
    }
  ]

  const parentOptions = (query.data ?? [])
    .filter((d) => d.id !== current?.id)
    .map((d) => ({ value: d.id, label: `${d.code} - ${d.name}` }))

  return (
    <PageContainer
      title='Cơ cấu tổ chức'
      actions={
        canCreate && (
          <PrimaryButtonComponent icon={<Plus size={16} />} onClick={() => setEditing('new')}>
            Thêm đơn vị
          </PrimaryButtonComponent>
        )
      }
    >
      {query.isPending && <SkeletonTableComponent />}
      {query.isError && (
        <ErrorState
          message={query.error instanceof ApiError ? query.error.message : 'Vui lòng thử lại.'}
          onRetry={() => void query.refetch()}
        />
      )}
      {query.isSuccess && tree.length === 0 && <EmptyStateComponent description='Chưa có đơn vị nào.' />}
      {query.isSuccess && tree.length > 0 && (
        <TableComponent<DepartmentTreeNode>
          rowKey='id'
          columns={columns}
          dataSource={tree}
          pagination={false}
          expandable={{ defaultExpandAllRows: true }}
          showActionColumn={canUpdate || canDelete}
          actionColumn={{
            width: 280,
            render: (department) => (
              <div className='flex items-center gap-1'>
                {canUpdate && (
                  <>
                    <ButtonComponent
                      type='link'
                      size='small'
                      icon={<Pencil size={14} />}
                      onClick={() => setEditing(department)}
                    >
                      Sửa
                    </ButtonComponent>
                    <ConfirmAction
                      label={department.isActive ? 'Ngừng' : 'Hoạt động lại'}
                      title={`${department.isActive ? 'Ngừng hoạt động' : 'Hoạt động lại'} đơn vị ${department.name}?`}
                      onConfirm={async () => {
                        await departmentApi.setActive(department.id, !department.isActive)
                        await refresh()
                      }}
                    />
                  </>
                )}
                {canDelete && (
                  <ConfirmAction
                    label='Xóa'
                    icon={<Trash2 size={14} />}
                    title={`Xóa đơn vị ${department.name}?`}
                    danger
                    confirmLabel='Xóa'
                    successMessage='Đã xóa đơn vị.'
                    onConfirm={async () => {
                      await departmentApi.remove(department.id)
                      await refresh()
                    }}
                  />
                )}
              </div>
            )
          }}
        />
      )}

      <FormDrawer
        open={editing !== null}
        title={current ? `Sửa đơn vị ${current.code}` : 'Thêm đơn vị'}
        initialValues={current ? { ...emptyForm, ...current } : emptyForm}
        validationSchema={schema}
        fields={[
          { name: 'code', label: 'Mã đơn vị', required: true, disabled: !!current },
          { name: 'name', label: 'Tên đơn vị', required: true },
          { name: 'parentId', label: 'Đơn vị cấp trên', type: 'select', options: parentOptions },
          { name: 'typeCode', label: 'Loại đơn vị', type: 'select', options: typeOptions },
          { name: 'sortOrder', label: 'Thứ tự', type: 'number', required: true }
        ]}
        onClose={() => setEditing(null)}
        onSubmit={async ({ code, name, parentId, typeCode, sortOrder }) => {
          const body = { name, parentId: parentId || null, typeCode: typeCode || null, sortOrder: Number(sortOrder) }
          if (current) await departmentApi.update(current.id, { ...body, version: current.version })
          else await departmentApi.create({ ...body, code })
          await refresh()
        }}
      />
    </PageContainer>
  )
}
