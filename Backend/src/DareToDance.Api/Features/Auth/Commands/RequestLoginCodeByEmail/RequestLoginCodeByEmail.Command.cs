using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByEmail;

public static partial class RequestLoginCodeByEmail
{
    public sealed record Command(string Email) : IRequest<ErrorOr<Success>>;

    public sealed class Handler(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IOptions<OtpSettings> otpOptions,
        IEmailSender emailSender)
        : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var email = command.Email.Trim().ToLowerInvariant();

            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user is null)
            {
                // Ne otkrivamo da li nalog postoji - sprecava account enumeration
                // (neko bi mogao da proba veliki broj mejlova i po odgovoru zakljuci koji su registrovani).
                return Result.Success;
            }

            return await RequestLoginCodeHelper.RequestCodeAsync(
                dbContext,
                passwordHasher,
                otpOptions.Value,
                user,
                email,
                emailSender.SendLoginCodeAsync,
                cancellationToken);
        }
    }
}
