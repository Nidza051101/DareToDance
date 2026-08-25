import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import OtpRequestPage from './pages/OtpRequestPage.tsx'
import { initTheme } from './theme.ts'

initTheme()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <OtpRequestPage />
  </StrictMode>,
)
