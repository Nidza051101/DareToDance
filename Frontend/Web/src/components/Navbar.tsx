import { useState } from 'react'
import AuthService from '../services/AuthService'
import { getAccessToken } from '../services/authToken'
import logo from '../assets/logo.png'
import { getStoredTheme, getSystemTheme, setTheme, type Theme } from '../theme'
import './Navbar.css'

function isActivePage(fileName: string) {
  const path = window.location.pathname

  if (fileName === 'index.html') {
    return path === '/' || path.endsWith('/index.html')
  }

  return path.endsWith(`/${fileName}`)
}

function Navbar() {
  const [isLoggedIn] = useState(() => Boolean(getAccessToken()))
  const [theme, setThemeState] = useState<Theme>(() => getStoredTheme() ?? getSystemTheme())

  const handleLogout = async () => {
    // refreshToken se ne šalje odavde — backend ga sam pokupi iz HttpOnly
    // kolačića (browser ga automatski priloži jer je withCredentials: true).
    await AuthService.logout()
    window.location.href = '/index.html'
  }

  const toggleTheme = () => {
    const next: Theme = theme === 'dark' ? 'light' : 'dark'
    setTheme(next)
    setThemeState(next)
  }

  return (
    <nav className="navbar">
      <div className="navbar-left">
        <a href="/home.html" className="navbar-brand">
          <img src={logo} alt="Dare to Dance" className="navbar-logo" />
        </a>
        <div className="navbar-links">
          <a href="/home.html" className={isActivePage('home.html') ? 'active' : undefined}>
            Home
          </a>
          {!isLoggedIn && (
            <a href="/index.html" className={isActivePage('index.html') ? 'active' : undefined}>
              Login
            </a>
          )}
          <a href="/users.html" className={isActivePage('users.html') ? 'active' : undefined}>
            Users
          </a>
        </div>
      </div>
      <div className="navbar-right">
        <button
          type="button"
          className="theme-toggle"
          onClick={toggleTheme}
          aria-label={theme === 'dark' ? 'Switch to light theme' : 'Switch to dark theme'}
        >
          {theme === 'dark' ? '☀️' : '🌙'}
        </button>
        {isLoggedIn && (
          <button type="button" className="btn" onClick={handleLogout}>
            Log out
          </button>
        )}
      </div>
    </nav>
  )
}

export default Navbar
