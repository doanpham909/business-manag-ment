import {
  EmptyStateComponent,
  InputIconComponent,
  SkeletonTableComponent,
  TableComponent,
  TablePaginationComponent,
  type ConfigurableColumn,
  type TableActionColumn
} from '@systemvci/shared-ui'
import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { useEffect, useState, type ReactNode } from 'react'
import type { PagedResult } from '@/shared/api/api-envelope'
import { ApiError } from '@/shared/api/api-error'
import { ErrorState } from '@/shared/components/error-state'

export interface ResourceListParams {
  pageIndex: number
  pageSize: number
  searchTerm?: string
}

interface ResourceTableProps<T extends { id: string }> {
  /** Query key gốc, ví dụ ['admin', 'companies'] */
  queryKey: readonly unknown[]
  fetchPage: (params: ResourceListParams) => Promise<PagedResult<T>>
  columns: ConfigurableColumn<T>[]
  actionColumn?: TableActionColumn<T>
  searchPlaceholder?: string
  /** Bộ lọc thêm đặt cạnh ô tìm kiếm */
  filters?: ReactNode
  emptyDescription?: string
}

const DEFAULT_PAGE_SIZE = 20

/** Bảng danh sách chuẩn: tìm kiếm, phân trang ở server, đủ 4 trạng thái (tải / lỗi / rỗng / có dữ liệu). */
export function ResourceTable<T extends { id: string }>({
  queryKey,
  fetchPage,
  columns,
  actionColumn,
  searchPlaceholder = 'Tìm theo mã hoặc tên',
  filters,
  emptyDescription = 'Chưa có dữ liệu.'
}: ResourceTableProps<T>) {
  const [pageIndex, setPageIndex] = useState(1)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
  const [search, setSearch] = useState('')
  const [searchTerm, setSearchTerm] = useState('')

  useEffect(() => {
    const timer = setTimeout(() => {
      setSearchTerm(search.trim())
      setPageIndex(1)
    }, 300)
    return () => clearTimeout(timer)
  }, [search])

  const query = useQuery({
    queryKey: [...queryKey, 'list', { pageIndex, pageSize, searchTerm }],
    queryFn: () => fetchPage({ pageIndex, pageSize, searchTerm: searchTerm || undefined })
  })

  return (
    <div className='flex flex-col gap-4'>
      <div className='flex flex-wrap items-center gap-2'>
        <InputIconComponent
          className='w-72'
          allowClear
          placeholder={searchPlaceholder}
          prefixIcon={<Search size={16} />}
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
        {filters}
      </div>

      {query.isPending && <SkeletonTableComponent />}
      {query.isError && (
        <ErrorState
          message={query.error instanceof ApiError ? query.error.message : 'Vui lòng thử lại.'}
          onRetry={() => void query.refetch()}
        />
      )}
      {query.isSuccess && query.data.items.length === 0 && <EmptyStateComponent description={emptyDescription} />}
      {query.isSuccess && query.data.items.length > 0 && (
        <>
          <TableComponent<T>
            rowKey='id'
            columns={columns}
            dataSource={query.data.items}
            actionColumn={actionColumn}
            showActionColumn={!!actionColumn}
            pagination={false}
            loading={query.isFetching}
          />
          <TablePaginationComponent
            current={query.data.pageIndex}
            pageSize={query.data.pageSize}
            total={query.data.totalCount}
            onCurrentChange={setPageIndex}
            onPageSizeChange={(size) => {
              setPageSize(size)
              setPageIndex(1)
            }}
          />
        </>
      )}
    </div>
  )
}
