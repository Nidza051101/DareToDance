using DareToDance.Infrastructure.Services;
using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<Logout.Command>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MaximumLength(RefreshTokenWireFormat.MaxWireLength);
    }
}
