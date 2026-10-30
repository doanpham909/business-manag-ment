import { ButtonComponent, ModalConfirmComponent, showStaticNotification } from '@systemvci/shared-ui'
import { useState, type ReactNode } from 'react'
import { ApiError } from '@/shared/api/api-error'

interface ConfirmActionProps {
  /** Nội dung nút mở hộp xác nhận */
  label: ReactNode
  icon?: ReactNode
  title: string
  content?: ReactNode
  confirmLabel?: string
  danger?: boolean
  successMessage?: string
  onConfirm: () => Promise<unknown>
}

/** Nút thao tác có hộp xác nhận (xóa, khóa, đổi trạng thái…). Lỗi API hiện bằng thông báo. */
export function ConfirmAction({
  label,
  icon,
  title,
  content,
  confirmLabel = 'Xác nhận',
  danger = false,
  successMessage = 'Đã thực hiện.',
  onConfirm
}: ConfirmActionProps) {
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)

  const handleConfirm = async () => {
    setLoading(true)
    try {
      await onConfirm()
      showStaticNotification({ variant: 'success', message: successMessage })
      setOpen(false)
    } catch (error) {
      showStaticNotification({
        variant: 'danger',
        message: error instanceof ApiError ? error.message : 'Không thực hiện được thao tác.'
      })
    } finally {
      setLoading(false)
    }
  }

  return (
    <>
      <ButtonComponent type='link' size='small' danger={danger} icon={icon} onClick={() => setOpen(true)}>
        {label}
      </ButtonComponent>
      <ModalConfirmComponent
        open={open}
        title={title}
        content={content}
        variant={danger ? 'danger' : 'primary'}
        onCancel={() => setOpen(false)}
        actions={[
          { key: 'cancel', label: 'Hủy', onClick: () => setOpen(false) },
          { key: 'ok', label: confirmLabel, type: 'primary', danger, loading, onClick: () => void handleConfirm() }
        ]}
      />
    </>
  )
}
