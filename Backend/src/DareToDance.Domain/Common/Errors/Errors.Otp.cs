using ErrorOr;

namespace DareToDance.Domain.Common.Errors;

public static partial class Errors
{
    public static class Otp
    {
        // invalid and expired share one message so responses don't reveal whether a code exists
        public static Error InvalidCode => Error.Unauthorized(
            code: "Otp.InvalidCode",
            description: "The code is invalid or has expired.");

        public static Error Expired => Error.Unauthorized(
            code: "Otp.Expired",
            description: "The code is invalid or has expired.");

        public static Error TooManyAttempts => Error.Forbidden(
            code: "Otp.TooManyAttempts",
            description: "Too many incorrect attempts. Request a new code.");

        public static Error ResendCooldown => Error.Conflict(
            code: "Otp.ResendCooldown",
            description: "A code was sent recently. Wait before requesting another.");
    }
}
