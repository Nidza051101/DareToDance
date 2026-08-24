using DareToDance.Domain.User;
using DareToDance.Infrastructure.Services;

namespace DareToDance.Api.Features.Auth.Shared;

public sealed record AuthResult(
    User User,
    AccessToken AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc)
{
    public override string ToString()
        => $"AuthResult {{ User = {User.Id.Value}, AccessToken = {AccessToken}, " +
           $"RefreshToken = [REDACTED], RefreshTokenExpiresAtUtc = {RefreshTokenExpiresAtUtc:O} }}";
}
