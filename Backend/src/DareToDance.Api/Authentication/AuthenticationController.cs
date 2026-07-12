using DareToDance.Api.Authentication.Contracts;
using DareToDance.Api.Common;
using DareToDance.Application.Services.Authentication;
using DareToDance.Application.Services.Authentication.Commands.InitiateLogin;
using DareToDance.Application.Services.Authentication.Commands.Register;
using DareToDance.Application.Services.Authentication.Commands.ResendOtp;
using DareToDance.Application.Services.Authentication.Commands.VerifyOtp;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Authentication;

[Route("auth")]
public class AuthenticationController(ISender mediator) : ApiController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.FirstName,
            request.LastName,
            request.Email);

        var result = await mediator.Send(command);

        return result.Match(challenge => Ok(MapToResponse(challenge)), Problem);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var command = new InitiateLoginCommand(request.Email);

        var result = await mediator.Send(command);

        return result.Match(challenge => Ok(MapToResponse(challenge)), Problem);
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
    {
        var command = new VerifyOtpCommand(request.Email, request.Code);

        var result = await mediator.Send(command);

        return result.Match(authResult => Ok(MapToResponse(authResult)), Problem);
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(ResendOtpRequest request)
    {
        var command = new ResendOtpCommand(request.Email);

        var result = await mediator.Send(command);

        return result.Match(challenge => Ok(MapToResponse(challenge)), Problem);
    }

    private static AuthenticationResponse MapToResponse(AuthenticationResult result) =>
        new(
            result.Id,
            result.FirstName,
            result.LastName,
            result.Email,
            result.Token);

    private static OtpChallengeResponse MapToResponse(OtpChallengeResult result) =>
        new(result.Message);
}
