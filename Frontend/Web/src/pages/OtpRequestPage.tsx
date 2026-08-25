import { useState } from 'react';
import AuthService from '../services/AuthService';
import Navbar from '../components/Navbar';
import '../App.css';

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
            window.location.href = `/verify.html?email=${encodeURIComponent(email.trim())}`;
        } catch (error) {
            console.error(error);
            setError('Unable to request OTP. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <>
            <Navbar />
            <section id="center">
                <div className="panel">
                    <h2>Login</h2>

                    <p>
                        Enter your email address to receive a verification code.
                    </p>

                    <input
                        id="email"
                        type="email"
                        className="field"
                        value={email}
                        onChange={(event) => setEmail(event.target.value)}
                        placeholder="Enter your email"
                        disabled={loading}
                    />

                    {error && (
                        <p className="error-text" role="alert">
                            {error}
                        </p>
                    )}

                    <button
                        type="button"
                        className="btn"
                        onClick={handleRequestOtp}
                        disabled={loading}
                    >
                        {loading ? 'Sending...' : 'Request OTP'}
                    </button>
                </div>
            </section>
        </>
    );
}
