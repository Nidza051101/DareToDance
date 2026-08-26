import { useState } from 'react';
import Navbar from '../components/Navbar';
import '../App.css';

export default function CompleteRegistrationPage() {
    const [phone, setPhone] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async () => {
        setError('');

        if (!phone.trim()) {
            setError('Phone is required.');
            return;
        }

        try {
            setLoading(true);
            // ovdje ide poziv servisa
        } catch (error) {
            console.error(error);
            setError('Unable to complete registration. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <>
            <Navbar />
            <section id="center">
                <div className="panel">
                    <h2>Complete Registration</h2>
                    <p>Enter your phone number to complete registration.</p>
                    <input
                        type="tel"
                        className="field"
                        value={phone}
                        onChange={(e) => setPhone(e.target.value)}
                        placeholder="Enter your phone number"
                        disabled={loading}
                    />
                    {error && <p className="error-text" role="alert">{error}</p>}
                    <button
                        type="button"
                        className="btn"
                        onClick={handleSubmit}
                        disabled={loading}
                    >
                        {loading ? 'Saving...' : 'Complete Registration'}
                    </button>
                </div>
            </section>
        </>
    );
}