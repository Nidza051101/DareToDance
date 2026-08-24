import { useEffect, useState } from 'react'
import authService from './services/auth.service'
import userService, { type UserResponse } from './services/user.service'
import './App.css'

function App() {
  const [userId, setUserId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const [lookupId, setLookupId] = useState('')
  const [lookupResult, setLookupResult] = useState<UserResponse | null>(null)
  const [lookupError, setLookupError] = useState<string | null>(null)

  const [otpEmail, setOtpEmail] = useState('')
  const [otpCode, setOtpCode] = useState('')
  const [otpError, setOtpError] = useState<string | null>(null)

  useEffect(() => {
    authService
      .getMe()
      .then((response) => setUserId(response.data.userId))
      .catch(() => setError('You are not logged in.'))
  }, [])

  const handleLogout = async () => {
    await authService.logout()
    setUserId(null)
    setError('You are not logged in.')
  }

  const handleVerify = async () => {
    setOtpError(null)

    try {
      const response = await authService.verifyOtp(otpEmail, otpCode)
      setUserId(response.data.userId)
      setError(null)
    } catch {
      setOtpError('Wrong or expired code.')
    }
  }

  const handleViewDetails = async () => {
    setLookupError(null)
    setLookupResult(null)

    try {
      const response = await userService.getUserById(lookupId)
      setLookupResult(response.data)
    } catch {
      setLookupError('User not found.')
    }
  }

  return (
    <section id="center">
      <h2>Verify OTP code</h2>
      <input
        type="text"
        value={otpEmail}
        onChange={(e) => setOtpEmail(e.target.value)}
        placeholder="Email"
      />
      <input
        type="text"
        value={otpCode}
        onChange={(e) => setOtpCode(e.target.value)}
        placeholder="6-digit code"
      />
      <button type="button" onClick={handleVerify}>
        Verify
      </button>
      {otpError && <p>{otpError}</p>}

      <h1>Logged in user</h1>
      {userId && <p>ID: {userId}</p>}
      {error && <p>{error}</p>}
      {userId && (
        <button type="button" onClick={handleLogout}>
          Log out
        </button>
      )}

      <h2>Look up a user</h2>
      <input
        type="text"
        value={lookupId}
        onChange={(e) => setLookupId(e.target.value)}
        placeholder="User ID"
      />
      <button type="button" onClick={handleViewDetails}>
        View details
      </button>

      {lookupError && <p>{lookupError}</p>}
      {lookupResult && (
        <ul>
          <li>Email: {lookupResult.email}</li>
          <li>First name: {lookupResult.firstName}</li>
          <li>Last name: {lookupResult.lastName}</li>
          <li>Phone: {lookupResult.phone ?? '—'}</li>
        </ul>
      )}
    </section>
  )
}

export default App
