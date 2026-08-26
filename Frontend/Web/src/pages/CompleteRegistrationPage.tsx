import { useState } from 'react'
import AuthService from '../services/AuthService'
import Navbar from '../components/Navbar'
import '../App.css'

function getParamsFromUrl() {
  const params = new URLSearchParams(window.location.search)
  return {
    idToken: params.get('idToken') ?? '',
    email: params.get('email') ?? '',
    firstName: params.get('firstName') ?? '',
    lastName: params.get('lastName') ?? '',
  }
}

export default function CompleteRegistrationPage() {
  const [{ idToken, email, firstName, lastName }] = useState(getParamsFromUrl)
  const [phone, setPhone] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const handleComplete = async () => {
    setError(null)

    if (!phone.trim()) {
      setError('Phone is required.')
      return
    }

    try {
      setLoading(true)
      await AuthService.completeRegistration(idToken, phone.trim())
      window.location.href = '/home.html'
    } catch {
      setError('Unable to complete registration. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <>
      <Navbar />
      <section id="center">
        <div className="panel">
          <h2>Complete registration</h2>
          <p>
            {firstName} {lastName} ({email})
          </p>

          <input
            type="tel"
            className="field"
            value={phone}
            onChange={(e) => setPhone(e.target.value)}
            placeholder="Phone number"
            disabled={loading}
          />

          <button type="button" className="btn" onClick={handleComplete} disabled={loading}>
            {loading ? 'Creating account...' : 'Complete registration'}
          </button>

          {error && <p className="error-text">{error}</p>}
        </div>
      </section>
    </>
  )
}
