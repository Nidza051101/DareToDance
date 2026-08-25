import { useState } from 'react'
import authService from '../services/auth.service'
import './Navbar.css'

function isActivePage(fileName: string) {
  const path = window.location.pathname

  if (fileName === 'index.html') {
    return path === '/' || path.endsWith('/index.html')
  }

  return path.endsWith(`/${fileName}`)
}

function Navbar() {
  const [isLoggedIn] = useState(() => Boolean(localStorage.getItem('accessToken')))

  const handleLogout = async () => {
    await authService.logout()
    window.location.href = '/index.html'
  }

  return (
    <nav className="navbar">
      <div className="navbar-links">
        <a href="/home.html" className={isActivePage('home.html') ? 'active' : undefined}>
          Home
        </a>
        <a href="/index.html" className={isActivePage('index.html') ? 'active' : undefined}>
          Verify
        </a>
        <a href="/users.html" className={isActivePage('users.html') ? 'active' : undefined}>
          Users
        </a>
      </div>
      {isLoggedIn && (
        <button type="button" className="btn" onClick={handleLogout}>
          Log out
        </button>
      )}
    </nav>
  )
}

export default Navbar
