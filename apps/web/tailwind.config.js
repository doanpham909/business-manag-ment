import { createRequire } from 'node:module'
import path from 'node:path'
import sharedUiPreset from '@systemvci/shared-ui/tailwind-preset'

// Tìm thư mục dist của shared-ui qua module resolution (đúng cả khi npm hoist dependency lên thư mục cha).
const require = createRequire(import.meta.url)
const sharedUiDist = path.dirname(require.resolve('@systemvci/shared-ui'))

/** @type {import('tailwindcss').Config} */
export default {
  // Preset của shared-ui: map màu thương hiệu (bg-main, text-main, border...) sang CSS variable.
  presets: [sharedUiPreset],
  content: [
    './index.html',
    './src/**/*.{ts,tsx}',
    // Bắt buộc: quét bundle shared-ui để sinh các utility class mà component của kit sử dụng.
    path.join(sharedUiDist, '**/*.{js,cjs}').replace(/\\/g, '/')
  ],
  // Tắt preflight để không đè style của antd; reset dùng antd/dist/reset.css (giống QMS).
  corePlugins: { preflight: false },
  theme: {
    extend: {
      colors: {
        primary: 'rgb(var(--color-primary-rgb) / <alpha-value>)'
      }
    }
  },
  plugins: []
}
