import { Spin } from 'antd'

/** Spinner toàn màn hình — dùng khi tải route đầu tiên (lazy chunk). */
export function FullPageLoader() {
  return (
    <div className='flex h-screen items-center justify-center'>
      <Spin size='large' />
    </div>
  )
}
