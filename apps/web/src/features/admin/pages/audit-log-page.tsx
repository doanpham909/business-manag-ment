import { StatusTagComponent, type ConfigurableColumn } from '@systemvci/shared-ui'
import { PageContainer } from '@/shared/components/page-container'
import { ResourceTable } from '@/shared/components/resource-table'
import { formatDateTime } from '@/shared/utils/format-date'
import { adminKeys, auditLogApi } from '../api/admin-api'
import type { AuditLog } from '../api/admin-types'

const ACTION_LABELS: Record<AuditLog['action'], string> = { Created: 'Thêm', Updated: 'Sửa', Deleted: 'Xóa' }

const columns: ConfigurableColumn<AuditLog>[] = [
  { title: 'Thời điểm', dataIndex: 'occurredAt', width: 170, render: (value: string) => formatDateTime(value) },
  { title: 'Loại dữ liệu', dataIndex: 'entityType', width: 180 },
  { title: 'Mã bản ghi', dataIndex: 'entityId', width: 320 },
  {
    title: 'Thao tác',
    dataIndex: 'action',
    width: 110,
    render: (action: AuditLog['action']) => (
      <StatusTagComponent
        status={action}
        label={ACTION_LABELS[action]}
        colorMap={{ Created: 'green', Updated: 'blue', Deleted: 'red' }}
      />
    )
  },
  { title: 'Giá trị thay đổi', dataIndex: 'changes', ellipsis: true }
]

/** ADM-08: xem nhật ký thay đổi (chỉ đọc). */
export default function AuditLogPage() {
  return (
    <PageContainer title='Nhật ký thay đổi'>
      <ResourceTable
        queryKey={adminKeys.auditLogs}
        fetchPage={auditLogApi.list}
        columns={columns}
        searchPlaceholder='Lọc đúng loại dữ liệu, ví dụ User'
        emptyDescription='Chưa có nhật ký.'
      />
    </PageContainer>
  )
}
