import { defineConfig, mergeConfig } from 'vitest/config'
import viteConfig from './vite.config.ts'

// Dùng lại cấu hình Vite (plugin react, alias '@' và 'antd/lib'), chỉ thêm phần test.
export default defineConfig((env) =>
  mergeConfig(viteConfig(env), {
    test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: ['./src/test/setup.ts'],
      css: false,
      include: ['src/**/*.test.{ts,tsx}'],
      // Cho Vite xử lý shared-ui (thay vì Node import trực tiếp) để alias 'antd/lib' → 'antd' có hiệu lực
      server: { deps: { inline: ['@systemvci/shared-ui'] } }
    }
  })
)
