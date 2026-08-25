import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import OtpRequestPage from './pages/OtpRequestPage.tsx'
import GoogleTestPage from './pages/GoogleTestPage.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <OtpRequestPage />
    <GoogleTestPage />
  </StrictMode>,
)