import { SkeletonComponent, StatusTagComponent, TypographyComponent } from '@systemvci/shared-ui'
import { Descriptions } from 'antd'
import { ErrorState } from '@/shared/components/error-state'
import { formatDateTime } from '@/shared/utils/format-date'
import { useSystemInfoQuery } from '../api/use-system-info-query'

/** Thẻ trạng thái API: minh hoạ đủ 3 trạng thái loading / error / data của một query. */
export function SystemInfoCard() {
  const { data, error, isPending, isError, isFetching, refetch } = useSystemInfoQuery()

  if (isPending) return <SkeletonComponent active paragraph={{ rows: 4 }} />

  if (isError) {
    return (
      <ErrorState
        title='Không kết nối được API'
        message={error.message}
        retrying={isFetching}
        onRetry={() => void refetch()}
      />
    )
  }

  return (
    <div className='flex flex-col gap-4'>
      <div className='flex items-center gap-3'>
        <TypographyComponent variant='heading-sm'>Trạng thái API</TypographyComponent>
        <StatusTagComponent status='active' label='Đang hoạt động' />
      </div>
      <Descriptions
        bordered
        column={1}
        size='middle'
        items={[
          { key: 'name', label: 'Ứng dụng', children: data.name },
          { key: 'version', label: 'Phiên bản', children: data.version },
          { key: 'environment', label: 'Môi trường', children: data.environment },
          { key: 'serverTime', label: 'Giờ máy chủ', children: formatDateTime(data.serverTime) }
        ]}
      />
    </div>
  )
}
