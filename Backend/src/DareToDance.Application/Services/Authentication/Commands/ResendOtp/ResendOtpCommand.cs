using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Authentication.Commands.ResendOtp;

public record ResendOtpCommand(string Email) : IRequest<ErrorOr<OtpChallengeResult>>;
