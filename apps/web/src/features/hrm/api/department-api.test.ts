import { describe, expect, it } from 'vitest'
import { buildDepartmentTree, type Department } from './department-api'

const dept = (id: string, parentId: string | null): Department => ({
  id,
  code: id,
  name: id,
  parentId,
  sortOrder: 0,
  isActive: true,
  version: 1
})

describe('buildDepartmentTree', () => {
  it('xếp đơn vị con vào đơn vị cha', () => {
    const tree = buildDepartmentTree([dept('a', null), dept('b', 'a'), dept('c', 'b')])
    expect(tree.map((n) => n.id)).toEqual(['a'])
    expect(tree[0].children?.[0].children?.[0].id).toBe('c')
  })

  it('đơn vị có cha không còn trong danh sách được đưa lên gốc', () => {
    expect(buildDepartmentTree([dept('b', 'mat')]).map((n) => n.id)).toEqual(['b'])
  })
})
