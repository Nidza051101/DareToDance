using DareToDance.Api.Features.Memberships.Shared;
using DareToDance.Domain.Membership;
using DareToDance.Domain.User.Id;
using DareToDance.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Memberships.Commands.CreateMembership;

public static partial class CreateMembership
{
    public sealed record Command(Guid UserId, DateTime ValidFrom, DateTime ValidTo) : IRequest<ErrorOr<Membership>>;

    public sealed class Handler(AppDbContext dbContext) : IRequestHandler<Command, ErrorOr<Membership>>
    {
        public async Task<ErrorOr<Membership>> Handle(Command command, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(command.UserId);

            var userExists = await dbContext.Users
                .AnyAsync(u => u.Id == userId, cancellationToken);

            if (!userExists)
            {
                return MembershipErrors.UserNotFound;
            }

            var membership = Membership.Create(userId, command.ValidFrom, command.ValidTo);

            dbContext.Memberships.Add(membership);
            await dbContext.SaveChangesAsync(cancellationToken);

            return membership;
        }
    }
}
