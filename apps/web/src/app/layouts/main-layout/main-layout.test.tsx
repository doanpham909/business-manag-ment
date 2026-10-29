import { screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { routeObjects } from '@/app/router/route-objects'
import { renderRoutes } from '@/test/render-with-providers'

// Dashboard gọi API thật → mock để test không phụ thuộc backend
vi.mock('@/features/dashboard/api/system-info-api', () => ({
  getSystemInfo: vi.fn().mockResolvedValue({
    name: 'QLDN API',
    version: '1.0.0',
    environment: 'Development',
    serverTime: '2026-10-29T08:00:00Z'
  })
}))

describe('MainLayout', () => {
  it('hiển thị menu đủ các module và dữ liệu trang Tổng quan', async () => {
    renderRoutes(routeObjects, '/')

    const menu = await screen.findByRole('menu')
    for (const label of ['Tổng quan', 'Quản trị hệ thống', 'Kho', 'Kế toán', 'Quản lý dự án']) {
      expect(within(menu).getByText(label)).toBeInTheDocument()
    }
    expect(await screen.findByText('QLDN API')).toBeInTheDocument()
    expect(screen.getByText('Đang hoạt động')).toBeInTheDocument()
  })

  it('click menu điều hướng sang trang module và cập nhật breadcrumb', async () => {
    const user = userEvent.setup()
    const { router } = renderRoutes(routeObjects, '/')

    await user.click(within(await screen.findByRole('menu')).getByText('Kho'))

    expect(await screen.findByText('Chức năng đang được phát triển')).toBeInTheDocument()
    expect(router.state.location.pathname).toBe('/kho')
    expect(screen.getByRole('navigation')).toHaveTextContent('Kho')
  })

  it('URL không tồn tại → trang 404', async () => {
    renderRoutes(routeObjects, '/khong-ton-tai')
    expect(await screen.findByText('Trang bạn tìm không tồn tại hoặc đã bị di chuyển.')).toBeInTheDocument()
  })
})
