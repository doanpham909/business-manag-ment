import type { ReactNode } from 'react'
import { PageHeaderComponent } from '@systemvci/shared-ui'

interface PageContainerProps {
  title: string
  subtitle?: ReactNode
  actions?: ReactNode
  children: ReactNode
}

/** Khung chuẩn của một trang: tiêu đề (PageHeader của shared-ui) + vùng nội dung dạng card. */
export function PageContainer({ title, subtitle, actions, children }: PageContainerProps) {
  return (
    <div className='flex flex-col gap-4'>
      <PageHeaderComponent title={title} subtitle={subtitle} actions={actions} />
      <div className='rounded-xl border border-solid border-divider bg-bg-card p-6'>{children}</div>
    </div>
  )
}
