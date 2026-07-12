namespace DareToDance.Application.Services.Authentication.Otp;

public record GeneratedOtp(string Code, string CodeHash);

public interface IOtpCodeGenerator
{
    GeneratedOtp Generate();

    /// <summary>Constant-time comparison of a submitted code against a stored hash.</summary>
    bool Matches(string code, string codeHash);
}
