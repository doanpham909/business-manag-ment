import { getAntTheme } from '@systemvci/shared-ui'
import { QueryClientProvider, type QueryClient } from '@tanstack/react-query'
import { App as AntApp, ConfigProvider } from 'antd'
import viVN from 'antd/locale/vi_VN'
import dayjs from 'dayjs'
import 'dayjs/locale/vi'
import type { ReactNode } from 'react'
import { queryClient as defaultQueryClient } from '@/shared/api/query-client'

// Ngôn ngữ duy nhất của app là tiếng Việt → cố định locale cho dayjs (DatePicker, định dạng ngày)
dayjs.locale('vi')

/** Theme antd lấy từ shared-ui (giống QMS) → các sản phẩm trông đồng nhất. */
const antTheme = getAntTheme(false)

interface AppProvidersProps {
  children: ReactNode
  /** Cho phép test truyền QueryClient riêng (tránh cache dùng chung giữa các test) */
  queryClient?: QueryClient
}

/** Gom các provider toàn cục: antd (locale vi_VN + theme), antd App (message/notification/modal), React Query. */
export function AppProviders({ children, queryClient = defaultQueryClient }: AppProvidersProps) {
  return (
    <ConfigProvider locale={viVN} theme={antTheme}>
      <AntApp>
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
      </AntApp>
    </ConfigProvider>
  )
}
