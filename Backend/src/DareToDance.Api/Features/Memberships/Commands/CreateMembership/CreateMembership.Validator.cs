using FluentValidation;

namespace DareToDance.Api.Features.Memberships.Commands.CreateMembership;

public static partial class CreateMembership
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.UserId)
                .NotEmpty();

            RuleFor(c => c.ValidFrom)
                .NotEmpty();

            RuleFor(c => c.ValidTo)
                .NotEmpty()
                .GreaterThan(c => c.ValidFrom)
                .WithMessage("ValidTo must be after ValidFrom.");
        }
    }
}
