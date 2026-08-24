import { useRef, useState } from 'react'
import authService from '../services/auth.service'
import Navbar from '../components/Navbar'
import '../App.css'

const CODE_LENGTH = 6

function VerifyPage() {
  const [otpEmail, setOtpEmail] = useState('')
  const [digits, setDigits] = useState<string[]>(Array(CODE_LENGTH).fill(''))
  const [otpError, setOtpError] = useState<string | null>(null)
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
    setOtpError(null)

    try {
      await authService.verifyOtp(otpEmail, digits.join(''))
      window.location.href = '/home.html'
    } catch {
      setOtpError('Wrong or expired code.')
    }
  }

  return (
    <>
      <Navbar />
      <section id="center">
        <div className="panel">
          <h2>Verify OTP code</h2>
          <input
            type="text"
            className="field"
            value={otpEmail}
            onChange={(e) => setOtpEmail(e.target.value)}
            placeholder="Email"
          />
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
          <button type="button" className="counter" onClick={handleVerify}>
            Verify
          </button>
          {otpError && <p className="error-text">{otpError}</p>}
        </div>
      </section>
    </>
  )
}

export default VerifyPage
