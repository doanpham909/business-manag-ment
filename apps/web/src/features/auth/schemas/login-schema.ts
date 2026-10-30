import * as Yup from 'yup'

export interface LoginFormValues {
  username: string
  password: string
}

export const loginInitialValues: LoginFormValues = { username: '', password: '' }

export const loginSchema = Yup.object({
  username: Yup.string().trim().required('Vui lòng nhập tên đăng nhập.').max(100, 'Tối đa 100 ký tự.'),
  password: Yup.string().required('Vui lòng nhập mật khẩu.').max(200, 'Tối đa 200 ký tự.')
})
