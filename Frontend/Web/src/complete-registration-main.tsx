import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import CompleteRegistrationPage from './pages/CompleteRegistrationPage.tsx'
import { initTheme } from './theme.ts'

initTheme()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <CompleteRegistrationPage />
  </StrictMode>,
)
