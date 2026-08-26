import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import OtpVerifyPage from './pages/OtpVerifyPage.tsx'
import { initTheme } from './theme.ts'

initTheme()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <OtpVerifyPage />
  </StrictMode>,
)
