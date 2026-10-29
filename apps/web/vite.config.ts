import { fileURLToPath, URL } from 'node:url'
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

/** Backend mặc định khi chạy local (apps/api). Ghi đè bằng VITE_API_PROXY_TARGET trong .env.local. */
const DEFAULT_API_PROXY_TARGET = 'http://localhost:5080'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), 'VITE_')
  const proxyTarget = env.VITE_API_PROXY_TARGET || DEFAULT_API_PROXY_TARGET
  // Dev server chuyển tiếp các request này sang backend → FE gọi cùng origin, không vướng CORS.
  const proxyEntry = { target: proxyTarget, changeOrigin: true }

  return {
    plugins: [react()],
    resolve: {
      alias: [
        { find: '@', replacement: fileURLToPath(new URL('./src', import.meta.url)) },
        // shared-ui 0.1.9 import `theme` từ 'antd/lib' (bản CommonJS): vừa lỗi trong Node ESM (Vitest),
        // vừa kéo thêm cả bộ antd CJS vào bundle. Trỏ về 'antd' (ESM, export cùng `theme`) để dùng chung 1 bản.
        { find: /^antd\/lib$/, replacement: 'antd' }
      ]
    },
    build: {
      // Chunk vendor (antd + shared-ui) ~720 kB minified là bình thường với antd → nâng ngưỡng cảnh báo
      chunkSizeWarningLimit: 1024
    },
    server: {
      port: 4200,
      strictPort: true,
      proxy: {
        '/api': proxyEntry,
        '/health': proxyEntry
      }
    },
    preview: {
      port: 4200,
      strictPort: true
    }
  }
})
