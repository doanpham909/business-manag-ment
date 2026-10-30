import {
  PrimaryButtonComponent,
  StatusTagComponent,
  ButtonComponent,
  type ConfigurableColumn
} from '@systemvci/shared-ui'
import { useQueryClient } from '@tanstack/react-query'
import { Pencil, Plus } from 'lucide-react'
import { useState } from 'react'
import * as Yup from 'yup'
import { usePermission } from '@/shared/auth/auth-store'
import { ConfirmAction } from '@/shared/components/confirm-action'
import { FormDrawer } from '@/shared/components/form-drawer'
import type { FormField } from '@/shared/components/form-field-control'
import { PageContainer } from '@/shared/components/page-container'
import { ResourceTable } from '@/shared/components/resource-table'
import { adminKeys, companyApi } from '../api/admin-api'
import type { Company } from '../api/admin-types'

const schema = Yup.object({
  code: Yup.string().trim().required('Vui lòng nhập mã công ty.').max(50, 'Tối đa 50 ký tự.'),
  name: Yup.string().trim().required('Vui lòng nhập tên công ty.').max(255, 'Tối đa 255 ký tự.'),
  taxCode: Yup.string().nullable().max(20, 'Tối đa 20 ký tự.'),
  address: Yup.string().nullable().max(500, 'Tối đa 500 ký tự.'),
  phone: Yup.string().nullable().max(20, 'Tối đa 20 ký tự.')
})

const emptyForm = { code: '', name: '', taxCode: '', address: '', phone: '' }

const columns: ConfigurableColumn<Company>[] = [
  { title: 'Mã', dataIndex: 'code', width: 120 },
  { title: 'Tên công ty', dataIndex: 'name' },
  { title: 'Mã số thuế', dataIndex: 'taxCode', width: 150 },
  { title: 'Địa chỉ', dataIndex: 'address' },
  {
    title: 'Trạng thái',
    dataIndex: 'status',
    width: 160,
    render: (status: Company['status']) => (
      <StatusTagComponent
        status={status}
        label={status === 'Active' ? 'Đang hoạt động' : 'Ngừng hoạt động'}
        colorMap={{ Active: 'green', Inactive: 'gray' }}
      />
    )
  }
]

/** ADM-00: danh sách công ty trong khách hàng. */
export default function CompanyListPage() {
  const queryClient = useQueryClient()
  const canCreate = usePermission('admin.company.create')
  const canUpdate = usePermission('admin.company.update')
  const [editing, setEditing] = useState<Company | 'new' | null>(null)

  const refresh = () => queryClient.invalidateQueries({ queryKey: adminKeys.companies })
  const current = editing && editing !== 'new' ? editing : null

  const fields: FormField[] = [
    { name: 'code', label: 'Mã công ty', required: true, disabled: !!current },
    { name: 'name', label: 'Tên công ty', required: true },
    { name: 'taxCode', label: 'Mã số thuế' },
    { name: 'address', label: 'Địa chỉ' },
    { name: 'phone', label: 'Điện thoại' }
  ]

  return (
    <PageContainer
      title='Công ty'
      actions={
        canCreate && (
          <PrimaryButtonComponent icon={<Plus size={16} />} onClick={() => setEditing('new')}>
            Thêm công ty
          </PrimaryButtonComponent>
        )
      }
    >
      <ResourceTable
        queryKey={adminKeys.companies}
        fetchPage={companyApi.list}
        columns={columns}
        actionColumn={
          canUpdate
            ? {
                width: 240,
                render: (company) => (
                  <div className='flex items-center gap-1'>
                    <ButtonComponent
                      type='link'
                      size='small'
                      icon={<Pencil size={14} />}
                      onClick={() => setEditing(company)}
                    >
                      Sửa
                    </ButtonComponent>
                    <ConfirmAction
                      label={company.status === 'Active' ? 'Ngừng hoạt động' : 'Hoạt động lại'}
                      title={`${company.status === 'Active' ? 'Ngừng hoạt động' : 'Hoạt động lại'} công ty ${company.name}?`}
                      content={
                        company.status === 'Active'
                          ? 'Người dùng sẽ không chọn được công ty này để làm việc. Dữ liệu cũ được giữ nguyên.'
                          : undefined
                      }
                      danger={company.status === 'Active'}
                      onConfirm={async () => {
                        await companyApi.setActive(company.id, company.status !== 'Active')
                        await refresh()
                      }}
                    />
                  </div>
                )
              }
            : undefined
        }
      />

      <FormDrawer
        open={editing !== null}
        title={current ? `Sửa công ty ${current.code}` : 'Thêm công ty'}
        initialValues={current ? { ...emptyForm, ...current } : emptyForm}
        validationSchema={schema}
        fields={fields}
        onClose={() => setEditing(null)}
        onSubmit={async (values) => {
          const { code, name, taxCode, address, phone } = values
          if (current) await companyApi.update(current.id, { name, taxCode, address, phone, version: current.version })
          else await companyApi.create({ code, name, taxCode, address, phone })
          await refresh()
        }}
      />
    </PageContainer>
  )
}
