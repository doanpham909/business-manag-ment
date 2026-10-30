import { describe, expect, it } from 'vitest'
import type { AppRoute } from '@/shared/types/app-route'
import { buildMenuItems } from './menu-utils'

const routes: AppRoute[] = [
  { path: '/', title: 'Tổng quan' },
  {
    path: '/quan-tri',
    title: 'Quản trị',
    children: [
      { path: '/quan-tri/a', title: 'A', permission: 'admin.a.view' },
      { path: '/quan-tri/b', title: 'B', permission: 'admin.b.view' }
    ]
  }
]

const keys = (items: ReturnType<typeof buildMenuItems>) => items.map((item) => item?.key)

describe('buildMenuItems', () => {
  it('ẩn mục không có quyền, giữ mục không yêu cầu quyền', () => {
    const items = buildMenuItems(routes, (permission) => !permission || permission === 'admin.a.view')
    expect(keys(items)).toEqual(['/', '/quan-tri'])
    expect((items[1] as { children: unknown[] }).children).toHaveLength(1)
  })

  it('ẩn cả nhóm khi không còn mục con nào được thấy', () => {
    expect(keys(buildMenuItems(routes, (permission) => !permission))).toEqual(['/'])
  })
})
