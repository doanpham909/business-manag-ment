import dayjs from 'dayjs'

/** Định dạng ngày giờ kiểu Việt Nam (vd 29/10/2026 14:05:09). Trả về chuỗi gốc nếu không parse được. */
export function formatDateTime(value: string | Date): string {
  const date = dayjs(value)
  return date.isValid() ? date.format('DD/MM/YYYY HH:mm:ss') : String(value)
}

/** Định dạng ngày kiểu Việt Nam (vd 29/10/2026). Trả về chuỗi gốc nếu không parse được. */
export function formatDate(value: string | Date): string {
  const date = dayjs(value)
  return date.isValid() ? date.format('DD/MM/YYYY') : String(value)
}
