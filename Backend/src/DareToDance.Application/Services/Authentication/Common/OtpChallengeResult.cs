namespace DareToDance.Application.Services.Authentication;

public record OtpChallengeResult(string Message)
{
    public static OtpChallengeResult CodeSent =>
        new("If the email address is valid, a sign-in code has been sent.");
}
