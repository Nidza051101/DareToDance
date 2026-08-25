import { useEffect, useRef, useState } from 'react';
import AuthService from '../services/AuthService';
import Navbar from '../components/Navbar';
import '../App.css';

export default function OtpRequestPage() {
    const [email, setEmail] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const googleButtonRef = useRef<HTMLDivElement>(null);

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

    // Google credential is a signed JWT containing the token our backend
    // needs to verify (idToken) - never inspected here, only forwarded.
    const handleGoogleCredential = async (idToken: string) => {
        setError('');

        try {
            const result = await AuthService.loginWithGoogle(idToken);

            if (result.status === 'loggedIn') {
                window.location.href = '/home.html';
                return;
            }

            const { email, firstName, lastName } = result.identity;
            const query = new URLSearchParams({ idToken, email, firstName, lastName });
            window.location.href = `/complete-registration.html?${query.toString()}`;
        } catch (err) {
            console.error(err);
            setError('Unable to sign in with Google. Please try again.');
        }
    };

    // Korak 4 (script) je već učitan u index.html sa async/defer, pa se ne
    // garantuje da je window.google spreman čim se ova komponenta prikaže -
    // sačekamo ga umesto da pretpostavimo da već postoji.
    useEffect(() => {
        const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;

        const tryInit = () => {
            if (!window.google || !googleButtonRef.current) {
                return false;
            }

            window.google.accounts.id.initialize({
                client_id: clientId,
                callback: (response) => {
                    void handleGoogleCredential(response.credential);
                },
            });

            // GIS renders a fixed-pixel-width button (no percentage support),
            // so match it to the actual container width instead of a magic
            // number - keeps it aligned with the "Request OTP" button above.
            window.google.accounts.id.renderButton(googleButtonRef.current, {
                theme: 'outline',
                size: 'large',
                text: 'continue_with',
                width: googleButtonRef.current.offsetWidth,
            });

            return true;
        };

        if (tryInit()) {
            return;
        }

        const interval = setInterval(() => {
            if (tryInit()) {
                clearInterval(interval);
            }
        }, 100);

        return () => clearInterval(interval);
    }, []);

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

                    <div className="divider">
                        <span>or</span>
                    </div>

                    <div className="google-button" ref={googleButtonRef}></div>
                </div>
            </section>
        </>
    );
}
