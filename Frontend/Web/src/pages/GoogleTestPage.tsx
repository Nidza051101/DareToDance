import { useEffect } from 'react'
import AuthService from '../services/AuthService'

export default function GoogleTestPage() {
  useEffect(() => {
    console.log('ORIGIN:', window.location.origin)
    console.log('CLIENT ID:', import.meta.env.VITE_GOOGLE_CLIENT_ID)

    ;(window as any).google.accounts.id.initialize({
      client_id: import.meta.env.VITE_GOOGLE_CLIENT_ID,
      callback: async (response: any) => {
        try {
          await AuthService.googleLogin(response.credential)
          window.location.href = '/home.html'
        } catch (error: any) {
          if (error.response?.status === 404) {
            window.location.href = '/complete-registration.html'
          } else {
            console.error(error)
          }
        }
      },
    })

    ;(window as any).google.accounts.id.renderButton(
      document.getElementById('google-btn'),
      { theme: 'outline', size: 'large' }
    )
  }, [])

  return (
    <div>
      <h2>Test Google Login</h2>
      <div id="google-btn"></div>
    </div>
  )
}