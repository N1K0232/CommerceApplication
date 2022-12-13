using System.Security.Claims;
using CommerceApi.Authentication.Entities;
using CommerceApi.Authentication.Extensions;
using CommerceApi.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CommerceApi.Authorization.Handlers;

public class UserActiveHandler : AuthorizationHandler<UserActiveRequirement>
{
    private readonly UserManager<AuthenticationUser> userManager;

    public UserActiveHandler(UserManager<AuthenticationUser> userManager)
    {
        this.userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserActiveRequirement requirement)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var userId = context.User.GetId();
            var user = await userManager.FindByIdAsync(userId.ToString());
            var lockoutEnd = user.LockoutEnd.GetValueOrDefault();
            var securityStamp = context.User.GetClaimValue(ClaimTypes.SerialNumber);

            if (user is not null && lockoutEnd <= DateTimeOffset.UtcNow && securityStamp == user.SecurityStamp)
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