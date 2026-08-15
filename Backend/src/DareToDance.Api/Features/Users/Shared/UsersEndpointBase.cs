using DareToDance.Api.Common.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Users.Shared;

[Route("users")]
[Tags("Users")]
public abstract class UsersEndpointBase : ApiEndpointBase;
