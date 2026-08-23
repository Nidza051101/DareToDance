using DareToDance.Domain.User;
using DareToDance.Infrastructure.Services;

namespace DareToDance.Api.Features.Auth.Shared;

public sealed record AuthResult(User User, AccessToken AccessToken);
