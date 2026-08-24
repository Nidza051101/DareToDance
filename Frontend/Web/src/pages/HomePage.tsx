import { useEffect, useState } from 'react'
import authService from '../services/auth.service'
import Navbar from '../components/Navbar'
import '../App.css'

function HomePage() {
  const [userId, setUserId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    authService
      .getMe()
      .then((response) => setUserId(response.data.userId))
      .catch(() => setError('You are not logged in.'))
  }, [])

  return (
    <>
      <Navbar />
      <section id="center">
        <div className="panel">
          <h2>Logged in user</h2>
          {userId && <p>ID: {userId}</p>}
          {error && <p className="error-text">{error}</p>}
        </div>
      </section>
    </>
  )
}

export default HomePage
