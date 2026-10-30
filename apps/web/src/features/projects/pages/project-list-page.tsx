import {
  ButtonComponent,
  PrimaryButtonComponent,
  StatusTagComponent,
  type ConfigurableColumn
} from '@systemvci/shared-ui'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Copy, Pencil, Plus, Trash2 } from 'lucide-react'
import { useState } from 'react'
import * as Yup from 'yup'
import { useCatalogOptions, useDepartmentOptions } from '@/shared/api/option-queries'
import { usePermission } from '@/shared/auth/auth-store'
import { ConfirmAction } from '@/shared/components/confirm-action'
import { FormDrawer } from '@/shared/components/form-drawer'
import type { FormField } from '@/shared/components/form-field-control'
import { PageContainer } from '@/shared/components/page-container'
import { ResourceTable } from '@/shared/components/resource-table'
import { formatDate } from '@/shared/utils/format-date'
import { PROJECT_STATUS_LABELS, projectApi, projectKeys, type Project } from '../api/project-api'
import { ProjectStatusActions } from '../components/project-status-actions'

const schema = Yup.object({
  code: Yup.string().trim().required('Vui lòng nhập mã dự án.').max(50, 'Tối đa 50 ký tự.'),
  name: Yup.string().trim().required('Vui lòng nhập tên dự án.').max(255, 'Tối đa 255 ký tự.'),
  location: Yup.string().nullable().max(500, 'Tối đa 500 ký tự.'),
  managerName: Yup.string().nullable().max(200, 'Tối đa 200 ký tự.'),
  endDate: Yup.string()
    .nullable()
    .test('end-after-start', 'Ngày kết thúc không được trước ngày bắt đầu.', function check(value) {
      const start = this.parent.startDate as string | null
      return !value || !start || value >= start
    })
})
const copySchema = Yup.object({
  newCode: Yup.string().trim().required('Vui lòng nhập mã dự án mới.').max(50, 'Tối đa 50 ký tự.')
})

const emptyForm = {
  code: '',
  name: '',
  parentProjectId: null as string | null,
  projectTypeCode: null as string | null,
  constructionTypeCode: null as string | null,
  location: '',
  departmentId: null as string | null,
  managerName: '',
  startDate: null as string | null,
  endDate: null as string | null,
  description: ''
}

/** PRJ-01: danh mục dự án — thêm, sửa, xóa, sao chép, đổi trạng thái. */
export default function ProjectListPage() {
  const queryClient = useQueryClient()
  const canCreate = usePermission('projects.project.create')
  const canUpdate = usePermission('projects.project.update')
  const canDelete = usePermission('projects.project.delete')
  const projectTypes = useCatalogOptions('project-type')
  const constructionTypes = useCatalogOptions('construction-type')
  const departments = useDepartmentOptions()
  const parents = useQuery({
    queryKey: [...projectKeys.all, 'options'],
    queryFn: () => projectApi.list({ pageIndex: 1, pageSize: 100 })
  })
  const [editing, setEditing] = useState<Project | 'new' | null>(null)
  const [copying, setCopying] = useState<Project | null>(null)

  const refresh = () => queryClient.invalidateQueries({ queryKey: projectKeys.all })
  const current = editing && editing !== 'new' ? editing : null

  const columns: ConfigurableColumn<Project>[] = [
    { title: 'Mã dự án', dataIndex: 'code', width: 160 },
    { title: 'Tên dự án', dataIndex: 'name' },
    {
      title: 'Dự án cha',
      dataIndex: 'parentProjectId',
      width: 160,
      render: (id: string | null) => parents.data?.items.find((p) => p.id === id)?.code
    },
    { title: 'Người phụ trách', dataIndex: 'managerName', width: 180 },
    { title: 'Bắt đầu', dataIndex: 'startDate', width: 110, render: (d: string | null) => d && formatDate(d) },
    { title: 'Kết thúc', dataIndex: 'endDate', width: 110, render: (d: string | null) => d && formatDate(d) },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      width: 140,
      render: (status: Project['status']) => (
        <StatusTagComponent
          status={status}
          label={PROJECT_STATUS_LABELS[status]}
          colorMap={{ InProgress: 'blue', Completed: 'green', Closed: 'gray' }}
        />
      )
    }
  ]

  const parentOptions = (parents.data?.items ?? [])
    .filter((p) => p.id !== current?.id)
    .map((p) => ({ value: p.id, label: `${p.code} - ${p.name}` }))

  const fields: FormField[] = [
    { name: 'code', label: 'Mã dự án', required: true, disabled: !!current },
    { name: 'name', label: 'Tên dự án', required: true },
    {
      name: 'parentProjectId',
      label: 'Dự án cha (nếu là dự án con / gói thầu)',
      type: 'select',
      options: parentOptions
    },
    { name: 'projectTypeCode', label: 'Loại dự án', type: 'select', options: projectTypes },
    { name: 'constructionTypeCode', label: 'Loại công trình', type: 'select', options: constructionTypes },
    { name: 'departmentId', label: 'Đơn vị phụ trách', type: 'select', options: departments },
    { name: 'managerName', label: 'Người phụ trách' },
    { name: 'location', label: 'Địa điểm' },
    { name: 'startDate', label: 'Ngày bắt đầu', type: 'date' },
    { name: 'endDate', label: 'Ngày kết thúc dự kiến', type: 'date' },
    { name: 'description', label: 'Mô tả', type: 'textarea' }
  ]

  return (
    <PageContainer
      title='Danh mục dự án'
      actions={
        canCreate && (
          <PrimaryButtonComponent icon={<Plus size={16} />} onClick={() => setEditing('new')}>
            Thêm dự án
          </PrimaryButtonComponent>
        )
      }
    >
      <ResourceTable
        queryKey={projectKeys.all}
        fetchPage={projectApi.list}
        columns={columns}
        actionColumn={{
          width: 340,
          render: (project) => (
            <div className='flex flex-wrap items-center gap-1'>
              {canUpdate && (
                <ButtonComponent
                  type='link'
                  size='small'
                  icon={<Pencil size={14} />}
                  onClick={() => setEditing(project)}
                >
                  Sửa
                </ButtonComponent>
              )}
              {canCreate && (
                <ButtonComponent type='link' size='small' icon={<Copy size={14} />} onClick={() => setCopying(project)}>
                  Sao chép
                </ButtonComponent>
              )}
              {canUpdate && <ProjectStatusActions project={project} onChanged={refresh} />}
              {canDelete && (
                <ConfirmAction
                  label='Xóa'
                  icon={<Trash2 size={14} />}
                  title={`Xóa dự án ${project.code}?`}
                  content='Chỉ xóa được dự án chưa có dự án con và chưa có chứng từ.'
                  danger
                  confirmLabel='Xóa'
                  successMessage='Đã xóa dự án.'
                  onConfirm={async () => {
                    await projectApi.remove(project.id)
                    await refresh()
                  }}
                />
              )}
            </div>
          )
        }}
      />

      <FormDrawer
        open={editing !== null}
        width={640}
        title={current ? `Sửa dự án ${current.code}` : 'Thêm dự án'}
        initialValues={current ? { ...emptyForm, ...current } : emptyForm}
        validationSchema={schema}
        fields={fields}
        onClose={() => setEditing(null)}
        onSubmit={async ({ code, ...values }) => {
          const body = {
            name: values.name,
            parentProjectId: values.parentProjectId || null,
            projectTypeCode: values.projectTypeCode || null,
            constructionTypeCode: values.constructionTypeCode || null,
            location: values.location || null,
            departmentId: values.departmentId || null,
            managerName: values.managerName || null,
            startDate: values.startDate || null,
            endDate: values.endDate || null,
            description: values.description || null
          }
          if (current) await projectApi.update(current.id, { ...body, version: current.version })
          else await projectApi.create({ ...body, code })
          await refresh()
        }}
      />

      <FormDrawer
        open={!!copying}
        title={`Sao chép dự án ${copying?.code ?? ''}`}
        initialValues={{ newCode: '' }}
        validationSchema={copySchema}
        fields={[{ name: 'newCode', label: 'Mã dự án mới', required: true }]}
        successMessage='Đã sao chép dự án.'
        onClose={() => setCopying(null)}
        onSubmit={async ({ newCode }) => {
          await projectApi.copy(copying!.id, newCode)
          await refresh()
        }}
      />
    </PageContainer>
  )
}
