import { showStaticNotification } from '@systemvci/shared-ui'
import type { FormikHelpers } from 'formik'
import { ApiError } from './api-error'

/** Lỗi API của form: lỗi theo trường gắn vào field, còn lại hiện thông báo. */
export function applyApiError<V>(error: unknown, helpers: Pick<FormikHelpers<V>, 'setFieldError'>) {
  const apiError = error instanceof ApiError ? error : null
  const details = apiError?.details ?? []
  details.forEach((detail) => {
    const field = detail.code.charAt(0).toLowerCase() + detail.code.slice(1)
    helpers.setFieldError(field, detail.message)
  })
  if (!details.length) {
    showStaticNotification({ variant: 'danger', message: apiError?.message ?? 'Không lưu được dữ liệu.' })
  }
}
