using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByPhone;

public static partial class RequestLoginCodeByPhone
{
    public sealed record Command(string Phone) : IRequest<ErrorOr<Success>>;

    public sealed class Handler(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IOptions<OtpSettings> otpOptions,
        ISmsSender smsSender)
        : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var phone = User.NormalizePhone(command.Phone)!;

            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Phone == phone, cancellationToken);

            if (user is null)
            {
                // Ne otkrivamo da li nalog postoji - sprecava account enumeration.
                return Result.Success;
            }

            return await RequestLoginCodeHelper.RequestCodeAsync(
                dbContext,
                passwordHasher,
                otpOptions.Value,
                user,
                phone,
                smsSender.SendLoginCodeAsync,
                cancellationToken);
        }
    }
}
