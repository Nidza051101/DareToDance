import { useEffect, useState } from 'react'
import AuthService from '../services/AuthService'
import CompleteRegistrationPage from './CompleteRegistrationPage'

export default function GoogleTestPage() {
  const [completeRegistration, setCompleteRegistration] = useState(false)

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
          console.error('STATUS:', error.response?.status)

          console.error(
            'VALIDATION ERRORS:',
            JSON.stringify(error.response?.data?.errors, null, 2)
          )

          console.error('ERROR:', error)

          if (error.response?.status === 404) {
            setCompleteRegistration(true)
          }
        }
      },
    })

    ;(window as any).google.accounts.id.renderButton(
      document.getElementById('google-btn'),
      {
        theme: 'outline',
        size: 'large',
      }
    )
  }, [])

  if (completeRegistration) {
    return <CompleteRegistrationPage />
  }

  return (
    <div>
      <h2>Test Google Login</h2>
      <div id="google-btn"></div>
    </div>
  )
}
