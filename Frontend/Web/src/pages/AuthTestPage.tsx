import { useState } from 'react';
import AuthService from '../services/AuthService';

export default function AuthTestPage() {
    const [email, setEmail] = useState('');
    const [code, setCode] = useState('');
    const [message, setMessage] = useState('');
    const [isOtpRequested, setIsOtpRequested] = useState(false);
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [loading, setLoading] = useState(false);

    const handleRequestOtp = async () => {
        try {
            setLoading(true);
            setMessage('');

            await AuthService.requestOtp(email);

            setIsOtpRequested(true);
            setMessage('OTP code has been sent.');
        } catch (error) {
            console.error(error);
            setMessage('Failed to request OTP.');
        } finally {
            setLoading(false);
        }
    };

    const handleVerifyOtp = async () => {
        try {
            setLoading(true);
            setMessage('');

            await AuthService.verifyOtp(email, code);

            setIsLoggedIn(true);
            setMessage('Successfully logged in.');
        } catch (error) {
            console.error(error);
            setMessage('Invalid or expired OTP code.');
        } finally {
            setLoading(false);
        }
    };

    const handleLogout = async () => {
        try {
            const refreshToken = localStorage.getItem('refreshToken');

            if (refreshToken) {
                await AuthService.logout(refreshToken);
            } else {
                localStorage.clear();
            }

            setIsLoggedIn(false);
            setIsOtpRequested(false);
            setCode('');
            setMessage('Successfully logged out.');
        } catch (error) {
            console.error(error);
            localStorage.clear();
            setIsLoggedIn(false);
            setMessage('Logged out.');
        }
    };

    const handleGetMe = async () => {
        try {
            const result = await AuthService.getMe();

            setMessage(`Authenticated. User ID: ${result.userId}`);
        } catch (error) {
            console.error(error);
            setMessage('Failed to get current user.');
        }
    };

    return (
        <div style={{ padding: '40px', maxWidth: '500px', margin: '0 auto' }}>
            <h1>Authentication Test</h1>

            {!isLoggedIn ? (
                <>
                    <div style={{ marginBottom: '20px' }}>
                        <label>Email</label>
                        <input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="test2@test.local"
                            style={{
                                display: 'block',
                                width: '100%',
                                padding: '8px',
                                marginTop: '5px',
                            }}
                        />
                    </div>

                    {!isOtpRequested ? (
                        <button
                            onClick={handleRequestOtp}
                            disabled={loading || !email}
                        >
                            {loading ? 'Sending...' : 'Request OTP'}
                        </button>
                    ) : (
                        <>
                            <div style={{ marginBottom: '20px' }}>
                                <label>OTP Code</label>
                                <input
                                    type="text"
                                    value={code}
                                    onChange={(e) => setCode(e.target.value)}
                                    placeholder="Enter OTP code"
                                    maxLength={6}
                                    style={{
                                        display: 'block',
                                        width: '100%',
                                        padding: '8px',
                                        marginTop: '5px',
                                    }}
                                />
                            </div>

                            <button
                                onClick={handleVerifyOtp}
                                disabled={loading || !code}
                            >
                                {loading ? 'Verifying...' : 'Verify OTP'}
                            </button>
                        </>
                    )}
                </>
            ) : (
                <>
                    <p>Successfully logged in.</p>

                    <button onClick={handleGetMe}>
                        Get Me
                    </button>

                    <button
                        onClick={handleLogout}
                        style={{ marginLeft: '10px' }}
                    >
                        Logout
                    </button>
                </>
            )}

            {message && (
                <p style={{ marginTop: '20px' }}>
                    {message}
                </p>
            )}
        </div>
    );
}