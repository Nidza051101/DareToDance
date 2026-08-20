using DareToDance.Api.Features.Users.Shared;
using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using DareToDance.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Users.Queries.GetUserById;

public static class GetUserById
{
    public sealed record Query(Guid Id) : IRequest<ErrorOr<User>>;

    public sealed class Handler(AppDbContext dbContext)
        : IRequestHandler<Query, ErrorOr<User>>
    {
        public async Task<ErrorOr<User>> Handle(Query query, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(query.Id);

            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
            {
                return UserErrors.NotFound;
            }

            return user;
        }
    }
}
