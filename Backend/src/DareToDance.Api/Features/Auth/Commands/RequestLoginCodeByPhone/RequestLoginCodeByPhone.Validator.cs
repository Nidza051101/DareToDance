using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByPhone;

public sealed class RequestLoginCodeByPhoneCommandValidator : AbstractValidator<RequestLoginCodeByPhoneCommand>
{
    public RequestLoginCodeByPhoneCommandValidator()
    {
        RuleFor(c => c.Phone)
            .NotEmpty()
            .MaximumLength(30);
    }
}
