using CommerceApi.Authentication.Common;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class MeController : ControllerBase
{
    private readonly IAuthenticatedService _authenticatedService;

    public MeController(IAuthenticatedService authenticatedService)
    {
        _authenticatedService = authenticatedService;
    }

    [HttpPost("youraddress")]
    [RoleAuthorize(RoleNames.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddAddress([FromBody] SaveAddressRequest address)
    {
        var result = await _authenticatedService.AddAddressAsync(address);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("addphoto")]
    [RoleAuthorize(RoleNames.User)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddPhoto(IFormFile file)
    {
        var addPhotoResult = await _authenticatedService.AddPhotoAsync(file.FileName, file.OpenReadStream());
        return CreateResponse(addPhotoResult, StatusCodes.Status200OK);
    }

    [HttpGet("accountinfo")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User, RoleNames.Customer)]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get()
    {
        var user = await _authenticatedService.GetAsync();
        return Ok(user);
    }

    [HttpGet("getphoto")]
    [RoleAuthorize(RoleNames.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPhoto()
    {
        var contentResult = await _authenticatedService.GetPhotoAsync();
        return CreateResponse(contentResult, StatusCodes.Status200OK);
    }
}