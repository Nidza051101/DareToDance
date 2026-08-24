import { useEffect, useState } from 'react'
import authService from './services/auth.service'
import './App.css'

function App() {
  const [userId, setUserId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    authService
      .getMe()
      .then((response) => setUserId(response.data.userId))
      .catch(() => setError('You are not logged in.'))
  }, [])

  return (
    <section id="center">
      <h1>Logged in user</h1>
      {userId && <p>ID: {userId}</p>}
      {error && <p>{error}</p>}
    </section>
  )
}

export default App
