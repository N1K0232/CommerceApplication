using CommerceApi.Authentication.Common;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class UsersController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public UsersController(IAuthenticationService authenticationService)
    {
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
}