import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import HomePage from './pages/HomePage.tsx'
import { requireAuth } from './utils/requireAuth.ts'

if (requireAuth()) {
  createRoot(document.getElementById('root')!).render(
    <StrictMode>
      <HomePage />
    </StrictMode>,
  )
}
