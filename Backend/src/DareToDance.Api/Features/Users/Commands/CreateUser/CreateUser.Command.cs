using DareToDance.Api.Features.Users.Shared;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DareToDance.Api.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? Phone = null) : IRequest<ErrorOr<User>>
{

    public override string ToString()
        => $"CreateUserCommand {{ Email = {Email}, FirstName = {FirstName}, LastName = {LastName}, Phone = {Phone}, Password = [REDACTED] }}";
}

public sealed class CreateUserCommandHandler(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, ErrorOr<User>>
{
    public async Task<ErrorOr<User>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        if (await dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            return UserErrors.DuplicateEmail;
        }

        // TODO: PasswordHash je uklonjen iz User modela (nije njegova odgovornost) —
        // treba odluciti gde se cuva hash lozinke (npr. poseban Credentials entitet/tabela).
        // passwordHasher.Hash(command.Password) trenutno se ne perzistuje nigde.
        var user = User.Create(
            email,
            command.FirstName,
            command.LastName,
            command.Phone);

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

        return user;
    }
}
