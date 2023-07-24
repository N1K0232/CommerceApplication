using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.Shared.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var registerResult = await _identityService.RegisterAsync(registerRequest);
        return CreateResponse(registerResult, StatusCodes.Status200OK);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var loginResult = await _identityService.LoginAsync(loginRequest);
        return CreateResponse(loginResult, StatusCodes.Status200OK);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var refreshTokenResult = await _identityService.RefreshTokenAsync(refreshTokenRequest);
        return CreateResponse(refreshTokenResult, StatusCodes.Status200OK);
    }
}