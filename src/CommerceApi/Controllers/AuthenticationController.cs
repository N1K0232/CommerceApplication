using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Extensions;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class AuthenticationController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthenticationController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpDelete("DeleteAccount")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAccount(Guid userId)
    {
        var result = await _userService.DeleteAccountAsync(userId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _userService.LoginAsync(request);
        return CreateResponse(response, StatusCodes.Status200OK);
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _userService.RegisterAsync(request);
        return CreateResponse(response, StatusCodes.Status200OK);
    }

    [HttpPost("Refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await _userService.RefreshTokenAsync(request);
        return CreateResponse(response, StatusCodes.Status200OK);
    }

    [HttpPost("SignOut")]
    public async Task<IActionResult> Signout()
    {
        var email = User.GetEmail();

        var result = await _userService.SignOutAsync(email);
        return CreateResponse(result, StatusCodes.Status200OK);
    }
}