import { SelectComponent, useAntNotification } from '@systemvci/shared-ui'
import { useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import { ApiError } from '@/shared/api/api-error'
import { authApi } from '@/shared/auth/auth-api'
import { useAuthStore } from '@/shared/auth/auth-store'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'

const ALL = '__all__'

/** Đổi công ty đang làm việc ở header (server cấp token mới — ADR-0002). */
export function CompanySwitcher() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const notification = useAntNotification()
  const { session, setAuth } = useAuthStore()
  if (!session) return null

  const options = [
    ...session.companies.map((c) => ({ value: c.id, label: c.name })),
    ...(session.canViewAllCompanies ? [{ value: ALL, label: 'Tất cả công ty (chỉ xem)' }] : [])
  ]
  const value = session.allCompanies ? ALL : session.currentCompany?.id

  const handleChange = async (next: string) => {
    if (next === value) return
    try {
      setAuth(await authApi.selectCompany({ companyId: next === ALL ? null : next, allCompanies: next === ALL }))
      // Dữ liệu đang cache thuộc công ty cũ → bỏ hết
      queryClient.clear()
      navigate(ROUTE_PATHS.dashboard)
    } catch (e) {
      notification.open({ variant: 'danger', message: e instanceof ApiError ? e.message : 'Không đổi được công ty.' })
    }
  }

  return (
    <SelectComponent
      aria-label='Công ty đang làm việc'
      className='w-72'
      options={options}
      value={value}
      onChange={(next) => void handleChange(String(next))}
    />
  )
}
