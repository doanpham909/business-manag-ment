import {
  ButtonComponent,
  PrimaryButtonComponent,
  SelectComponent,
  SkeletonTableComponent,
  StatusTagComponent,
  type ConfigurableColumn
} from '@systemvci/shared-ui'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Pencil, Plus } from 'lucide-react'
import { useState } from 'react'
import * as Yup from 'yup'
import { usePermission } from '@/shared/auth/auth-store'
import { ConfirmAction } from '@/shared/components/confirm-action'
import { FormDrawer } from '@/shared/components/form-drawer'
import { PageContainer } from '@/shared/components/page-container'
import { ResourceTable } from '@/shared/components/resource-table'
import { adminKeys, catalogApi } from '../api/admin-api'
import type { CatalogItem } from '../api/admin-types'

const schema = Yup.object({
  code: Yup.string().trim().required('Vui lòng nhập mã.').max(50, 'Tối đa 50 ký tự.'),
  name: Yup.string().trim().required('Vui lòng nhập tên.').max(255, 'Tối đa 255 ký tự.'),
  description: Yup.string().nullable().max(1000, 'Tối đa 1000 ký tự.'),
  sortOrder: Yup.number().typeError('Thứ tự phải là số.').min(0, 'Thứ tự không âm.').required('Vui lòng nhập thứ tự.')
})

const emptyForm = { code: '', name: '', description: '', sortOrder: 0 }

const columns: ConfigurableColumn<CatalogItem>[] = [
  { title: 'Mã', dataIndex: 'code', width: 160 },
  { title: 'Tên', dataIndex: 'name' },
  { title: 'Mô tả', dataIndex: 'description' },
  { title: 'Thứ tự', dataIndex: 'sortOrder', width: 90, align: 'right' },
  {
    title: 'Trạng thái',
    dataIndex: 'isActive',
    width: 150,
    render: (isActive: boolean) => (
      <StatusTagComponent
        status={isActive ? 'active' : 'inactive'}
        label={isActive ? 'Đang sử dụng' : 'Ngừng sử dụng'}
        colorMap={{ active: 'green', inactive: 'gray' }}
      />
    )
  }
]

/** ADM-05: danh mục dùng chung cho cả tập đoàn. */
export default function CatalogPage() {
  const queryClient = useQueryClient()
  const canCreate = usePermission('admin.catalog.create')
  const canUpdate = usePermission('admin.catalog.update')
  const types = useQuery({ queryKey: [...adminKeys.catalogs, 'types'], queryFn: catalogApi.types, staleTime: Infinity })
  const [selected, setSelected] = useState<string>()
  const [editing, setEditing] = useState<CatalogItem | 'new' | null>(null)

  const type = selected ?? types.data?.[0]?.code
  const typeName = types.data?.find((t) => t.code === type)?.name ?? ''
  const refresh = () => queryClient.invalidateQueries({ queryKey: adminKeys.catalogs })
  const current = editing && editing !== 'new' ? editing : null

  return (
    <PageContainer
      title='Danh mục dùng chung'
      subtitle='Dùng cho mọi công ty trong tập đoàn'
      actions={
        canCreate &&
        type && (
          <PrimaryButtonComponent icon={<Plus size={16} />} onClick={() => setEditing('new')}>
            Thêm mục
          </PrimaryButtonComponent>
        )
      }
    >
      {!type && <SkeletonTableComponent />}
      {type && (
        <ResourceTable
          key={type}
          queryKey={[...adminKeys.catalogs, type]}
          fetchPage={(params) => catalogApi.list(type, params)}
          columns={columns}
          filters={
            <SelectComponent
              aria-label='Loại danh mục'
              className='w-64'
              options={(types.data ?? []).map((t) => ({ value: t.code, label: t.name }))}
              value={type}
              onChange={(next) => setSelected(next as string)}
            />
          }
          actionColumn={
            canUpdate
              ? {
                  width: 220,
                  render: (item) => (
                    <div className='flex items-center gap-1'>
                      <ButtonComponent
                        type='link'
                        size='small'
                        icon={<Pencil size={14} />}
                        onClick={() => setEditing(item)}
                      >
                        Sửa
                      </ButtonComponent>
                      <ConfirmAction
                        label={item.isActive ? 'Ngừng sử dụng' : 'Sử dụng lại'}
                        title={`${item.isActive ? 'Ngừng sử dụng' : 'Sử dụng lại'} "${item.name}"?`}
                        content={
                          item.isActive
                            ? 'Mục này sẽ không chọn được cho dữ liệu mới; chứng từ cũ vẫn hiển thị đúng.'
                            : undefined
                        }
                        onConfirm={async () => {
                          await catalogApi.setActive(item.id, !item.isActive)
                          await refresh()
                        }}
                      />
                    </div>
                  )
                }
              : undefined
          }
        />
      )}

      <FormDrawer
        open={editing !== null}
        title={current ? `Sửa ${current.name}` : `Thêm mục vào ${typeName}`}
        initialValues={current ? { ...emptyForm, ...current, description: current.description ?? '' } : emptyForm}
        validationSchema={schema}
        fields={[
          { name: 'code', label: 'Mã', required: true, disabled: !!current },
          { name: 'name', label: 'Tên', required: true },
          { name: 'description', label: 'Mô tả', type: 'textarea' },
          { name: 'sortOrder', label: 'Thứ tự', type: 'number', required: true }
        ]}
        onClose={() => setEditing(null)}
        onSubmit={async ({ code, name, description, sortOrder }) => {
          const body = { name, description: description || null, sortOrder: Number(sortOrder) }
          if (current) await catalogApi.update(current.id, { ...body, version: current.version })
          else await catalogApi.create({ ...body, type: type!, code })
          await refresh()
        }}
      />
    </PageContainer>
  )
}
