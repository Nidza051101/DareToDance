using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Authentication.Commands.VerifyOtp;

public record VerifyOtpCommand(string Email, string Code) : IRequest<ErrorOr<AuthenticationResult>>;
