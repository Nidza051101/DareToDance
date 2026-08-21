using DareToDance.Api.Common.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Memberships.Shared;

[Route("memberships")]
[Tags("Memberships")]
public abstract class MembershipsEndpointBase : ApiEndpointBase;
