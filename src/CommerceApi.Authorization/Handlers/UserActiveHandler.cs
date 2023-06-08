﻿using System.Security.Claims;
using CommerceApi.Authentication.Extensions;
using CommerceApi.Authentication.Managers;
using CommerceApi.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace CommerceApi.Authorization.Handlers;

public class UserActiveHandler : AuthorizationHandler<UserActiveRequirement>
{
    private readonly ApplicationUserManager _userManager;

    public UserActiveHandler(ApplicationUserManager userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserActiveRequirement requirement)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var userId = context.User.GetId();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var securityStamp = context.User.GetClaimValue(ClaimTypes.SerialNumber);

            if (user is not null && user.LockoutEnd.GetValueOrDefault() <= DateTimeOffset.UtcNow
                && securityStamp == user.SecurityStamp)
            {
                context.Succeed(requirement);
            }
            else
            {
                var reason = new AuthorizationFailureReason(this, "Invalid user");
                context.Fail(reason);
            }
        }
    }
}