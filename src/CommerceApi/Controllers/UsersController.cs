using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Extensions;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Models;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authenticationService;

    public UsersController(IUserService userService, IAuthenticationService authenticationService)
    {
        _userService = userService;
        _authenticationService = authenticationService;
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

    [HttpPost("uploadimage")]
    [Consumes("multipart/form-data")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    public async Task<IActionResult> UploadPhoto([FromForm] FormFileContent content)
    {
        var result = await _userService.UploadPhotoAsync(User.GetId(), content.File.FileName, content.GetFileStream());
        return CreateResponse(result, StatusCodes.Status200OK);
    }


    [HttpGet("Me")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMe()
    {
        var user = await _authenticationService.GetAsync();
        return Ok(user);
    }
}