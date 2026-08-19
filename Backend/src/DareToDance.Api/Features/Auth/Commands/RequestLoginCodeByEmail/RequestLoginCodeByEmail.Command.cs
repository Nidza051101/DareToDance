using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByEmail;

public sealed record RequestLoginCodeByEmailCommand(string Email) : IRequest<ErrorOr<Success>>;

public sealed class RequestLoginCodeByEmailCommandHandler(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IOptions<OtpSettings> otpOptions,
    IEmailSender emailSender)
    : RequestLoginCodeHandlerBase(dbContext, passwordHasher, otpOptions),
        IRequestHandler<RequestLoginCodeByEmailCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RequestLoginCodeByEmailCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            // Don't reveal whether the account exists - prevents account enumeration.
            return Result.Success;
        }

        return await RequestCodeAsync(user, email, cancellationToken);
    }

    protected override Task SendCodeAsync(string recipient, string code, CancellationToken cancellationToken)
        => emailSender.SendLoginCodeAsync(recipient, code, cancellationToken);
}
