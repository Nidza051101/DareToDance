using DareToDance.Api.Features.Users.Shared;
using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using DareToDance.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Users.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<ErrorOr<User>>;

public sealed class GetUserByIdQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetUserByIdQuery, ErrorOr<User>>
{
    public async Task<ErrorOr<User>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
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
