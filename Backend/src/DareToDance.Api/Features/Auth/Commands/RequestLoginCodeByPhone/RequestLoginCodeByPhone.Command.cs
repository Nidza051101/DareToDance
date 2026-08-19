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

public sealed record RequestLoginCodeByPhoneCommand(string Phone) : IRequest<ErrorOr<Success>>;

public sealed class RequestLoginCodeByPhoneCommandHandler(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IOptions<OtpSettings> otpOptions,
    ISmsSender smsSender)
    : RequestLoginCodeHandlerBase(dbContext, passwordHasher, otpOptions),
        IRequestHandler<RequestLoginCodeByPhoneCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RequestLoginCodeByPhoneCommand command, CancellationToken cancellationToken)
    {
        var phone = User.NormalizePhone(command.Phone)!;

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Phone == phone, cancellationToken);

        if (user is null)
        {
            // Don't reveal whether the account exists - prevents account enumeration.
            return Result.Success;
        }

        return await RequestCodeAsync(user, phone, cancellationToken);
    }

    protected override Task SendCodeAsync(string recipient, string code, CancellationToken cancellationToken)
        => smsSender.SendLoginCodeAsync(recipient, code, cancellationToken);
}
