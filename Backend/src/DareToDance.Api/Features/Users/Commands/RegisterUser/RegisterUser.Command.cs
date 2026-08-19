using DareToDance.Api.Features.Users.Shared;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DareToDance.Api.Features.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone) : IRequest<ErrorOr<User>>
{
    public override string ToString()
        => $"RegisterUserCommand {{ FirstName = {FirstName}, LastName = {LastName}, Email = {Email}, Phone = {Phone} }}";
}

public sealed class RegisterUserCommandHandler(AppDbContext dbContext)
    : IRequestHandler<RegisterUserCommand, ErrorOr<User>>
{
    public async Task<ErrorOr<User>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var phone = command.Phone.Trim();

        if (await dbContext.Users.AnyAsync(
                u => u.Email == email,
                cancellationToken))
        {
            return UserErrors.DuplicateEmail;
        }

        if (await dbContext.Users.AnyAsync(
                u => u.Phone == phone,
                cancellationToken))
        {
            return UserErrors.DuplicatePhone;
        }

        var user = User.Create(
            email,
            command.FirstName,
            command.LastName,
            phone);

        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException e) when (
            e.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "ix_users_email"
            })
        {
            return UserErrors.DuplicateEmail;
        }
        catch (DbUpdateException e) when (
            e.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "ix_users_phone"
            })
        {
            return UserErrors.DuplicatePhone;
        }

        return user;
    }
}
