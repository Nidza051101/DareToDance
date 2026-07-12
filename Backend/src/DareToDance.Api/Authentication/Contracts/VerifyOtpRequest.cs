namespace DareToDance.Api.Authentication.Contracts;

public record VerifyOtpRequest(
    string Email,
    string Code
);
