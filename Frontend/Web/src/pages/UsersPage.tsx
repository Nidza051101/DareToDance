import { useState } from 'react'
import userService, { type UserResponse } from '../services/user.service'
import Navbar from '../components/Navbar'
import '../App.css'

function UsersPage() {
  const [lookupId, setLookupId] = useState('')
  const [lookupResult, setLookupResult] = useState<UserResponse | null>(null)
  const [lookupError, setLookupError] = useState<string | null>(null)

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
    <>
      <Navbar />
      <section id="center">
        <div className="panel">
          <h2>Look up a user</h2>
          <input
            type="text"
            className="field"
            value={lookupId}
            onChange={(e) => setLookupId(e.target.value)}
            placeholder="User ID"
          />
          <button type="button" className="btn" onClick={handleViewDetails}>
            View details
          </button>

          {lookupError && <p className="error-text">{lookupError}</p>}
          {lookupResult && (
            <ul className="result-list">
              <li>Email: {lookupResult.email}</li>
              <li>First name: {lookupResult.firstName}</li>
              <li>Last name: {lookupResult.lastName}</li>
              <li>Phone: {lookupResult.phone ?? '—'}</li>
            </ul>
          )}
        </div>
      </section>
    </>
  )
}

export default UsersPage
