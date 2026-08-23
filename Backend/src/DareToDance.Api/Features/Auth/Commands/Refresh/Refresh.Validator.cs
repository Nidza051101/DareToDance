using DareToDance.Infrastructure.Services;
using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.Refresh;

public sealed class RefreshCommandValidator : AbstractValidator<Refresh.Command>
{
    public RefreshCommandValidator()
    {
        // Shape-only checks (400): anything that fits on the wire is judged by
        // the handler, where every failure collapses to the same generic 401.
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MaximumLength(RefreshTokenWireFormat.MaxWireLength);
    }
}
