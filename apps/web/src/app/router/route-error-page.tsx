import { ButtonComponent } from '@systemvci/shared-ui'
import { Result } from 'antd'
import { useRouteError } from 'react-router'

/** Error boundary cấp router: bắt lỗi render hoặc lỗi tải chunk (vd sau khi deploy bản mới). */
export function RouteErrorPage() {
  const error = useRouteError()
  const message = error instanceof Error ? error.message : 'Đã xảy ra lỗi không mong muốn.'

  return (
    <Result
      status='error'
      title='Có lỗi xảy ra'
      subTitle={message}
      extra={<ButtonComponent onClick={() => window.location.reload()}>Tải lại trang</ButtonComponent>}
    />
  )
}
