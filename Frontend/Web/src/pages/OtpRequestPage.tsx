import { useState } from 'react';
import AuthService from '../services/AuthService';

export default function OtpRequestPage() {
    const [email, setEmail] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleRequestOtp = async () => {
        setError('');

        if (!email.trim()) {
            setError('Email is required.');
            return;
        }

        try {
            setLoading(true);

            await AuthService.requestOtp(email.trim());
        } catch (error) {
            console.error(error);
            setError('Unable to request OTP. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="otp-request-page">
            <h1>Login</h1>

            <p>
                Enter your email address to receive a verification code.
            </p>

            <div>
                <label htmlFor="email">Email</label>

                <input
                    id="email"
                    type="email"
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    placeholder="Enter your email"
                    disabled={loading}
                />
            </div>

            {error && (
                <p role="alert">
                    {error}
                </p>
            )}

            <button
                type="button"
                onClick={handleRequestOtp}
                disabled={loading}
            >
                {loading ? 'Sending...' : 'Request OTP'}
            </button>
        </div>
    );
}