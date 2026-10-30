import { DrawerComponent, FormikErrorMessage, showStaticNotification } from '@systemvci/shared-ui'
import { Form, Formik, type FormikHelpers, type FormikProps, type FormikValues } from 'formik'
import type { ReactNode } from 'react'
import type { AnyObjectSchema } from 'yup'
import { applyApiError } from '@/shared/api/apply-api-error'
import { FormFieldControl, type FormField } from './form-field-control'

interface FormDrawerProps<V extends FormikValues> {
  open: boolean
  title: string
  initialValues: V
  validationSchema?: AnyObjectSchema
  fields?: FormField[]
  /** Nội dung form tự dựng, dùng khi `fields` không đủ */
  children?: (form: FormikProps<V>) => ReactNode
  submitLabel?: string
  width?: number
  successMessage?: string
  onSubmit: (values: V) => Promise<unknown>
  onClose: () => void
}

/** Form tạo/sửa trong drawer (≤ 8 trường theo design-system): Formik + Yup, hiện lỗi API, khóa nút khi đang gửi. */
export function FormDrawer<V extends FormikValues>({
  open,
  title,
  initialValues,
  validationSchema,
  fields,
  children,
  submitLabel = 'Lưu',
  width = 520,
  successMessage = 'Đã lưu.',
  onSubmit,
  onClose
}: FormDrawerProps<V>) {
  const handleSubmit = async (values: V, helpers: FormikHelpers<V>) => {
    try {
      await onSubmit(values)
      showStaticNotification({ variant: 'success', message: successMessage })
      onClose()
    } catch (error) {
      applyApiError(error, helpers)
    }
  }

  return (
    <Formik
      enableReinitialize
      initialValues={initialValues}
      validationSchema={validationSchema}
      onSubmit={handleSubmit}
    >
      {(form) => (
        <DrawerComponent
          open={open}
          title={title}
          width={width}
          destroyOnHidden
          onClose={onClose}
          actions={[
            { key: 'cancel', label: 'Hủy', onClick: onClose },
            {
              key: 'submit',
              label: submitLabel,
              type: 'primary',
              loading: form.isSubmitting,
              onClick: () => void form.submitForm()
            }
          ]}
        >
          <Form className='flex flex-col gap-4' noValidate>
            {fields?.map((field) => (
              <div key={field.name}>
                <FormFieldControl field={field} form={form} />
                <FormikErrorMessage name={field.name} />
              </div>
            ))}
            {children?.(form)}
          </Form>
        </DrawerComponent>
      )}
    </Formik>
  )
}
