using DareToDance.Api.Features.Users.Shared;
using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using DareToDance.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Users.Commands.UnblockUser;

public static partial class UnblockUser
{
    public sealed record Command(Guid Id) : IRequest<ErrorOr<User>>;

    public sealed class Handler(AppDbContext dbContext)
        : IRequestHandler<Command, ErrorOr<User>>
    {
        public async Task<ErrorOr<User>> Handle(Command command, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(command.Id);

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
            {
                return UserErrors.NotFound;
            }

            user.Activate(DateTime.UtcNow);

            await dbContext.SaveChangesAsync(cancellationToken);

            return user;
        }
    }
}
