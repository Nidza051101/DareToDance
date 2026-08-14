using DareToDance.Api.Common.Endpoints;
using DareToDance.Api.Common.Results;
using DareToDance.Api.Features.Users.Shared;
using MediatR;

namespace DareToDance.Api.Features.Users.CreateUser;

public sealed record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password)
{
    public override string ToString()
        => $"CreateUserRequest {{ Email = {Email}, FirstName = {FirstName}, LastName = {LastName}, Password = [REDACTED] }}";
}

public sealed class CreateUserEndpoint : IApiEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UserRoutes.Base,
                async (CreateUserRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(request.ToCommand(), cancellationToken);

                    return result.Match(
                        user => Results.Created(
                            $"{UserRoutes.Base}/{user.Id.Value}",
                            user.ToResponse()),
                        errors => errors.ToProblem());
                })
            .WithName("CreateUser")
            .WithTags("Users");
    }
}
