import { create } from 'zustand'
import { createJSONStorage, persist } from 'zustand/middleware'

interface LayoutState {
  /** Sidebar đang thu gọn hay không — lưu localStorage để giữ nguyên khi tải lại trang */
  siderCollapsed: boolean
  toggleSider: () => void
}

/** Client state của layout (zustand) — ví dụ chuẩn cho state UI dùng chung, không phải server state. */
export const useLayoutStore = create<LayoutState>()(
  persist(
    (set) => ({
      siderCollapsed: false,
      toggleSider: () => set((state) => ({ siderCollapsed: !state.siderCollapsed }))
    }),
    { name: 'qldn-layout', storage: createJSONStorage(() => localStorage) }
  )
)
