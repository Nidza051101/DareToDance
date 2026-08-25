import { useRef, useState } from 'react'
import AuthService from '../services/AuthService'
import Navbar from '../components/Navbar'
import '../App.css'

const CODE_LENGTH = 6

function getEmailFromUrl(): string {
  return new URLSearchParams(window.location.search).get('email') ?? ''
}

export default function OtpVerifyPage() {
  const [email] = useState(getEmailFromUrl)
  const [digits, setDigits] = useState<string[]>(Array(CODE_LENGTH).fill(''))
  const [error, setError] = useState<string | null>(null)
  const inputRefs = useRef<(HTMLInputElement | null)[]>([])

  const handleDigitChange = (index: number, value: string) => {
    if (!/^\d?$/.test(value)) {
      return
    }

    const next = [...digits]
    next[index] = value
    setDigits(next)

    if (value && index < CODE_LENGTH - 1) {
      inputRefs.current[index + 1]?.focus()
    }
  }

  const handleKeyDown = (index: number, e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Backspace' && !digits[index] && index > 0) {
      inputRefs.current[index - 1]?.focus()
    }
  }

  const handlePaste = (e: React.ClipboardEvent<HTMLInputElement>) => {
    const pasted = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, CODE_LENGTH)
    if (!pasted) {
      return
    }

    e.preventDefault()
    setDigits(Array.from({ length: CODE_LENGTH }, (_, i) => pasted[i] ?? ''))
    inputRefs.current[Math.min(pasted.length, CODE_LENGTH - 1)]?.focus()
  }

  const handleVerify = async () => {
    setError(null)

    try {
      await AuthService.verifyOtp(email, digits.join(''))
      window.location.href = '/home.html'
    } catch {
      setError('Wrong or expired code.')
    }
  }

  return (
    <>
      <Navbar />
      <section id="center">
        <div className="panel">
          <h2>Verify code</h2>
          {email && <p>Sent to: {email}</p>}
          <div className="otp-boxes">
            {digits.map((digit, index) => (
              <input
                key={index}
                ref={(el) => {
                  inputRefs.current[index] = el
                }}
                type="text"
                inputMode="numeric"
                maxLength={1}
                className="field otp-digit"
                value={digit}
                onChange={(e) => handleDigitChange(index, e.target.value)}
                onKeyDown={(e) => handleKeyDown(index, e)}
                onPaste={handlePaste}
              />
            ))}
          </div>
          <button type="button" className="btn" onClick={handleVerify}>
            Verify
          </button>
          {error && <p className="error-text">{error}</p>}
        </div>
      </section>
    </>
  )
}
