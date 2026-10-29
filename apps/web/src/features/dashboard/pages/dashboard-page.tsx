import { PageContainer } from '@/shared/components/page-container'
import { SystemInfoCard } from '../components/system-info-card'

/** Trang Tổng quan — Phase 0 chỉ hiển thị trạng thái kết nối tới backend. */
export default function DashboardPage() {
  return (
    <PageContainer title='Tổng quan' subtitle='Thông tin hệ thống'>
      <SystemInfoCard />
    </PageContainer>
  )
}
