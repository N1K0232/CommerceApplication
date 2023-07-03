using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Extensions;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Swagger.Models;
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

    [HttpPost("uploadimage")]
    [Consumes("multipart/form-data")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    public async Task<IActionResult> UploadPhoto([FromForm] FormFileContent content)
    {
        var result = await _userService.UploadPhotoAsync(User.GetId(), content.File.FileName, content.File.OpenReadStream());
        return CreateResponse(result, StatusCodes.Status200OK);
    }
}