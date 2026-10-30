import {
  FormikErrorMessage,
  InputIconComponent,
  PrimaryButtonComponent,
  TypographyComponent
} from '@systemvci/shared-ui'
import { Alert, Card, Input } from 'antd'
import { Form, Formik } from 'formik'
import { Lock, User } from 'lucide-react'
import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router'
import { ApiError } from '@/shared/api/api-error'
import { authApi } from '@/shared/auth/auth-api'
import { hasWorkingCompany, useAuthStore } from '@/shared/auth/auth-store'
import { APP_NAME } from '@/shared/constants/app-info'
import { ROUTE_PATHS } from '@/shared/constants/route-paths'
import { loginInitialValues, loginSchema, type LoginFormValues } from '../schemas/login-schema'

/** Đăng nhập bằng tên đăng nhập + mật khẩu (không nhập mã công ty — ADR-0002). */
export default function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { session, setAuth } = useAuthStore()
  const [error, setError] = useState<string | null>(null)
  const from = (location.state as { from?: string } | null)?.from ?? ROUTE_PATHS.dashboard

  if (session) return <Navigate to={hasWorkingCompany(session) ? from : ROUTE_PATHS.selectCompany} replace />

  const handleSubmit = async (values: LoginFormValues) => {
    setError(null)
    try {
      const response = await authApi.login({ username: values.username.trim(), password: values.password })
      setAuth(response)
      navigate(hasWorkingCompany(response.session) ? from : ROUTE_PATHS.selectCompany, { replace: true })
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Không đăng nhập được, vui lòng thử lại.')
    }
  }

  return (
    <div className='flex min-h-screen items-center justify-center bg-main p-4'>
      <Card className='w-full max-w-md'>
        <TypographyComponent variant='heading-md' className='mb-1 text-primary'>
          {APP_NAME}
        </TypographyComponent>
        <TypographyComponent variant='body' className='mb-6'>
          Đăng nhập để tiếp tục
        </TypographyComponent>

        {error && <Alert type='error' showIcon className='mb-4' message={error} />}

        <Formik initialValues={loginInitialValues} validationSchema={loginSchema} onSubmit={handleSubmit}>
          {({ values, handleChange, handleBlur, isSubmitting }) => (
            <Form className='flex flex-col gap-4' noValidate>
              <div>
                <InputIconComponent
                  name='username'
                  size='large'
                  placeholder='Tên đăng nhập'
                  autoComplete='username'
                  autoFocus
                  prefixIcon={<User size={16} />}
                  value={values.username}
                  onChange={handleChange}
                  onBlur={handleBlur}
                />
                <FormikErrorMessage name='username' />
              </div>
              <div>
                <Input.Password
                  name='password'
                  size='large'
                  placeholder='Mật khẩu'
                  autoComplete='current-password'
                  prefix={<Lock size={16} />}
                  value={values.password}
                  onChange={handleChange}
                  onBlur={handleBlur}
                />
                <FormikErrorMessage name='password' />
              </div>
              <PrimaryButtonComponent htmlType='submit' size='large' block loading={isSubmitting}>
                Đăng nhập
              </PrimaryButtonComponent>
            </Form>
          )}
        </Formik>
      </Card>
    </div>
  )
}
