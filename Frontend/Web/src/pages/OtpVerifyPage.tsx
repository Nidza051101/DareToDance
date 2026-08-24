interface OtpVerifyPageProps {
    email: string;
}

export default function OtpVerifyPage({ email }: OtpVerifyPageProps) {
    return (
        <div>
            <h1>Verify OTP</h1>

            <p>OTP verification page</p>
            <p>{email}</p>
        </div>
    );
}