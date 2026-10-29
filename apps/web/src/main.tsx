import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
// Thứ tự CSS: reset của antd → style của shared-ui → Tailwind + style riêng của app
import 'antd/dist/reset.css'
import '@systemvci/shared-ui/styles.css'
import './styles/index.css'
import { App } from './app/app'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>
)
