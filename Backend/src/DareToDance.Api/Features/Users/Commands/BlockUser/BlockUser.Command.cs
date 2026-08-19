using DareToDance.Api.Features.Users.Shared;
using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using DareToDance.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Users.Commands.BlockUser;

public sealed record BlockUserCommand(Guid Id) : IRequest<ErrorOr<User>>;

public sealed class BlockUserCommandHandler(AppDbContext dbContext)
    : IRequestHandler<BlockUserCommand, ErrorOr<User>>
{
    public async Task<ErrorOr<User>> Handle(BlockUserCommand command, CancellationToken cancellationToken)
    {
        var userId = UserId.Create(command.Id);

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return UserErrors.NotFound;
        }

        user.Block(DateTime.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}
