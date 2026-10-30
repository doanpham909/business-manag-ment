import {
  DefaultButtonComponent,
  EmptyStateComponent,
  TypographyComponent,
  useAntNotification
} from '@systemvci/shared-ui'
import { Card, List } from 'antd'
import { Building2, Layers } from 'lucide-react'
import { useState } from 'react'
import { Navigate, useNavigate } from 'react-router'
import { ApiError } from '@/shared/api/api-error'
import { authApi } from '@/shared/auth/auth-api'
import { useAuthStore } from '@/shared/auth/auth-store'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'

/** Chọn công ty làm việc sau khi đăng nhập (DEC-005), hoặc chế độ Tất cả công ty (chỉ đọc). */
export default function SelectCompanyPage() {
  const navigate = useNavigate()
  const notification = useAntNotification()
  const { session, setAuth, clear } = useAuthStore()
  const [pending, setPending] = useState<string | null>(null)

  if (!session) return <Navigate to={ROUTE_PATHS.login} replace />

  const choose = async (companyId: string | null) => {
    setPending(companyId ?? 'all')
    try {
      setAuth(await authApi.selectCompany({ companyId, allCompanies: companyId === null }))
      navigate(ROUTE_PATHS.dashboard, { replace: true })
    } catch (e) {
      notification.open({ variant: 'danger', message: e instanceof ApiError ? e.message : 'Không chọn được công ty.' })
    } finally {
      setPending(null)
    }
  }

  const logout = async () => {
    await authApi.logout().catch(() => undefined)
    clear()
    navigate(ROUTE_PATHS.login, { replace: true })
  }

  return (
    <div className='flex min-h-screen items-center justify-center bg-main p-4'>
      <Card className='w-full max-w-lg'>
        <TypographyComponent variant='heading-md' className='mb-1'>
          Chọn công ty làm việc
        </TypographyComponent>
        <TypographyComponent variant='body' className='mb-4'>
          Xin chào {session.user.fullName}. Dữ liệu bạn xem và nhập sẽ thuộc công ty được chọn.
        </TypographyComponent>

        {session.companies.length === 0 ? (
          <EmptyStateComponent description='Tài khoản chưa được cấp quyền vào công ty nào. Vui lòng liên hệ quản trị.' />
        ) : (
          <List
            bordered
            dataSource={session.companies}
            renderItem={(company) => (
              <List.Item
                actions={[
                  <DefaultButtonComponent
                    key='choose'
                    loading={pending === company.id}
                    onClick={() => choose(company.id)}
                  >
                    Chọn
                  </DefaultButtonComponent>
                ]}
              >
                <List.Item.Meta avatar={<Building2 size={20} />} title={company.name} description={company.code} />
              </List.Item>
            )}
          />
        )}

        <div className='mt-4 flex items-center justify-between gap-2'>
          {session.canViewAllCompanies ? (
            <DefaultButtonComponent
              icon={<Layers size={16} />}
              loading={pending === 'all'}
              onClick={() => choose(null)}
            >
              Tất cả công ty (chỉ xem)
            </DefaultButtonComponent>
          ) : (
            <span />
          )}
          <DefaultButtonComponent type='text' onClick={logout}>
            Đăng xuất
          </DefaultButtonComponent>
        </div>
      </Card>
    </div>
  )
}
