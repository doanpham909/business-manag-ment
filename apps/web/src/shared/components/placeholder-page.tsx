import { EmptyStateComponent } from '@systemvci/shared-ui'
import { PageContainer } from './page-container'

interface PlaceholderPageProps {
  title: string
  description?: string
}

/** Trang giữ chỗ cho module chưa triển khai (Phase 0). */
export function PlaceholderPage({ title, description }: PlaceholderPageProps) {
  return (
    <PageContainer title={title}>
      <EmptyStateComponent
        text='Chức năng đang được phát triển'
        description={description ?? 'Module này sẽ được triển khai trong các giai đoạn tiếp theo.'}
      />
    </PageContainer>
  )
}
