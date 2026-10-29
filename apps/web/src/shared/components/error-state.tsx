import { ButtonComponent } from '@systemvci/shared-ui'
import { Result } from 'antd'
import { RotateCw } from 'lucide-react'

interface ErrorStateProps {
  title?: string
  message: string
  onRetry?: () => void
  retrying?: boolean
}

/** Hiển thị lỗi tải dữ liệu kèm nút "Thử lại" (nút có trạng thái loading theo quy tắc FE). */
export function ErrorState({ title = 'Không tải được dữ liệu', message, onRetry, retrying }: ErrorStateProps) {
  return (
    <Result
      status='error'
      title={title}
      subTitle={message}
      extra={
        onRetry && (
          <ButtonComponent type='default' icon={<RotateCw size={16} />} loading={retrying} onClick={onRetry}>
            Thử lại
          </ButtonComponent>
        )
      }
    />
  )
}
