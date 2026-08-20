using DareToDance.Api.Common.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Auth.Shared;

[Route("auth")]
[Tags("Auth")]
public abstract class AuthEndpointBase : ApiEndpointBase;
