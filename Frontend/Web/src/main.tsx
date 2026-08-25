import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
//MORA SE ISPRAVITI
createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />

import VerifyPage from './pages/VerifyPage.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <VerifyPage />
  </StrictMode>,
)
