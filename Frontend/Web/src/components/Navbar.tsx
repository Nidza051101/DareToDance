import { useState } from 'react'
import authService from '../services/auth.service'
import './Navbar.css'

function Navbar() {
  const [isLoggedIn] = useState(() => Boolean(localStorage.getItem('accessToken')))

  const handleLogout = async () => {
    await authService.logout()
    window.location.href = '/index.html'
  }

  return (
    <nav className="navbar">
      <div className="navbar-links">
        <a href="/home.html">Home</a>
        <a href="/index.html">Verify</a>
        <a href="/users.html">Users</a>
      </div>
      {isLoggedIn && (
        <button type="button" className="counter" onClick={handleLogout}>
          Log out
        </button>
      )}
    </nav>
  )
}

export default Navbar
