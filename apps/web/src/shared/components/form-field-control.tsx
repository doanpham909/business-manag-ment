import { DatePickerComponent, InputIconComponent, SelectComponent, TypographyComponent } from '@systemvci/shared-ui'
import { Input, InputNumber } from 'antd'
import dayjs from 'dayjs'
import type { FormikProps, FormikValues } from 'formik'

export interface FormFieldOption {
  value: string
  label: string
}

export interface FormField {
  name: string
  label: string
  type?: 'text' | 'textarea' | 'number' | 'select' | 'date' | 'password'
  required?: boolean
  disabled?: boolean
  placeholder?: string
  options?: FormFieldOption[]
}

interface FormFieldControlProps<V extends FormikValues> {
  field: FormField
  form: FormikProps<V>
}

/** Nhãn + ô nhập theo loại trường. Ngày lưu dạng 'YYYY-MM-DD' (khớp DateOnly của backend). */
export function FormFieldControl<V extends FormikValues>({ field, form }: FormFieldControlProps<V>) {
  const value = form.values[field.name] as unknown
  const set = (next: unknown) => void form.setFieldValue(field.name, next)
  const common = { id: field.name, disabled: field.disabled, placeholder: field.placeholder }

  const renderControl = () => {
    switch (field.type) {
      case 'textarea':
        return (
          <Input.TextArea {...common} rows={3} value={(value as string) ?? ''} onChange={(e) => set(e.target.value)} />
        )
      case 'number':
        return <InputNumber {...common} className='w-full' value={value as number} onChange={(next) => set(next)} />
      case 'password':
        return <Input.Password {...common} value={(value as string) ?? ''} onChange={(e) => set(e.target.value)} />
      case 'select':
        return (
          <SelectComponent
            {...common}
            optionFilterProp='label'
            className='w-full'
            options={field.options ?? []}
            value={(value as string) || undefined}
            onChange={(next) => set(next ?? null)}
          />
        )
      case 'date':
        return (
          <DatePickerComponent
            {...common}
            className='w-full'
            format='DD/MM/YYYY'
            value={value ? dayjs(value as string) : null}
            onChange={(next) => set(next ? dayjs(next as dayjs.Dayjs).format('YYYY-MM-DD') : null)}
          />
        )
      default:
        return <InputIconComponent {...common} value={(value as string) ?? ''} onChange={(e) => set(e.target.value)} />
    }
  }

  return (
    <div className='flex flex-col gap-1'>
      <label htmlFor={field.name}>
        <TypographyComponent variant='body-medium' as='span'>
          {field.label}
          {field.required && <span className='text-danger'> *</span>}
        </TypographyComponent>
      </label>
      {renderControl()}
    </div>
  )
}
