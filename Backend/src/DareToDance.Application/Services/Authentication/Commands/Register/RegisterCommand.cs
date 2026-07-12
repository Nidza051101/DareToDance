using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Authentication.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email
) : IRequest<ErrorOr<OtpChallengeResult>>;
