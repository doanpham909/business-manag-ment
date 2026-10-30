import { ButtonComponent, TypographyComponent } from '@systemvci/shared-ui'
import { Alert, Layout, Spin } from 'antd'
import { PanelLeftClose, PanelLeftOpen } from 'lucide-react'
import { Outlet, useNavigation } from 'react-router'
import { appRoutes } from '@/app/router/route-config'
import { APP_NAME, APP_SHORT_NAME } from '@/shared/constants/app-info'
import { useAuthStore } from '@/shared/auth/auth-store'
import { AppBreadcrumb } from './app-breadcrumb'
import { CompanySwitcher } from './company-switcher'
import { PermissionGate } from './permission-gate'
import { SidebarMenu } from './sidebar-menu'
import { useLayoutStore } from './use-layout-store'
import { UserMenu } from './user-menu'

const { Sider, Header, Content } = Layout

/** Khung chính của app: sidebar menu + header + breadcrumb + nội dung trang (<Outlet/>). */
export function MainLayout() {
  const { siderCollapsed, toggleSider } = useLayoutStore()
  // Đang tải chunk của trang kế tiếp (lazy route) → hiện spinner trên vùng nội dung
  const isNavigating = useNavigation().state === 'loading'
  const readOnly = useAuthStore((state) => !!state.session?.allCompanies)

  return (
    <Layout className='h-screen'>
      <Sider
        width={248}
        collapsedWidth={72}
        collapsed={siderCollapsed}
        trigger={null}
        className='border-0 border-e border-solid border-divider'
      >
        <div className='flex h-14 items-center justify-center px-4'>
          <TypographyComponent variant='heading-sm' className='truncate text-primary'>
            {siderCollapsed ? APP_SHORT_NAME : APP_NAME}
          </TypographyComponent>
        </div>
        <SidebarMenu routes={appRoutes} collapsed={siderCollapsed} />
      </Sider>

      <Layout>
        <Header className='flex items-center gap-3 border-0 border-b border-solid border-divider px-4'>
          <ButtonComponent
            type='text'
            aria-label={siderCollapsed ? 'Mở rộng menu' : 'Thu gọn menu'}
            icon={siderCollapsed ? <PanelLeftOpen size={18} /> : <PanelLeftClose size={18} />}
            onClick={toggleSider}
          />
          <AppBreadcrumb />
          <div className='ms-auto flex items-center gap-2'>
            <CompanySwitcher />
            <UserMenu />
          </div>
        </Header>

        <Content className='overflow-auto p-6'>
          {readOnly && (
            <Alert
              type='info'
              showIcon
              className='mb-4'
              message='Đang xem Tất cả công ty — chế độ chỉ xem. Chọn một công ty cụ thể để thêm hoặc sửa dữ liệu.'
            />
          )}
          <Spin spinning={isNavigating} delay={150}>
            <PermissionGate>
              <Outlet />
            </PermissionGate>
          </Spin>
        </Content>
      </Layout>
    </Layout>
  )
}
