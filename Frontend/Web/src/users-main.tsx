import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import UsersPage from './pages/UsersPage.tsx'
import { requireAuth } from './utils/requireAuth.ts'

if (requireAuth()) {
  createRoot(document.getElementById('root')!).render(
    <StrictMode>
      <UsersPage />
    </StrictMode>,
  )
}
