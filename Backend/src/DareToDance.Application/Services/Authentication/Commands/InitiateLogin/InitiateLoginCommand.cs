using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Authentication.Commands.InitiateLogin;

public record InitiateLoginCommand(string Email) : IRequest<ErrorOr<OtpChallengeResult>>;
