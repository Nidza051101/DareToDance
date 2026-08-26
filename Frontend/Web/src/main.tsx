import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import GoogleTestPage from './pages/GoogleTestPage.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <GoogleTestPage />
  </StrictMode>,
)