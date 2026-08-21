using DareToDance.Api.Common.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Permissions.Shared;

[Route("permissions")]
[Tags("Permissions")]
public abstract class PermissionsEndpointBase : ApiEndpointBase;