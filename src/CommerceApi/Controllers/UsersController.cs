﻿using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Extensions;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class UsersController : ControllerBase
{
    private readonly IUserService userService;

    public UsersController(IUserService userService)
    {
        this.userService = userService;
    }


    [HttpDelete("DeleteAccount")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAccount(Guid userId)
    {
        var result = await userService.DeleteAccountAsync(userId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await userService.LoginAsync(request);
        return CreateResponse(response, StatusCodes.Status200OK);
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await userService.RegisterAsync(request);
        return CreateResponse(response, StatusCodes.Status200OK);
    }

    [HttpPost("Refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await userService.RefreshTokenAsync(request);
        return CreateResponse(response, StatusCodes.Status200OK);
    }

    [HttpPost("SignOut")]
    public async Task<IActionResult> Signout()
    {
        var email = User.GetEmail();

        var result = await userService.SignOutAsync(email);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("uploadimage")]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        var userId = User.GetId();
        var fileName = file.FileName;
        var stream = file.OpenReadStream();

        var result = await userService.UploadPhotoAsync(userId, fileName, stream);
        return CreateResponse(result, StatusCodes.Status200OK);
    }


    [HttpGet("Me")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetMe()
    {
        var user = new User
        {
            Id = User.GetId(),
            FirstName = User.GetFirstName(),
            LastName = User.GetLastName(),
            DateOfBirth = User.GetDateOfBirth(),
            Age = User.GetAge(),
            PhoneNumber = User.GetPhoneNumber(),
            Email = User.GetEmail(),
            UserName = User.GetUserName(),
            Roles = User.GetRoles()
        };

        return Ok(user);
    }
}