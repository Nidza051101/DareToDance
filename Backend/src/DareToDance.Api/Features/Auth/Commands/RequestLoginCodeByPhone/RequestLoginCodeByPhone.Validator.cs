using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByPhone;

public static partial class RequestLoginCodeByPhone
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Phone)
                .NotEmpty()
                .MaximumLength(30);
        }
    }
}
