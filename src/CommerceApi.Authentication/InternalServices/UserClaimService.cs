using System.Security.Claims;
using CommerceApi.Authentication.Extensions;
using CommerceApi.SharedServices;
using Microsoft.AspNetCore.Http;

namespace CommerceApi.Authentication.InternalServices;

internal class UserClaimService : IUserClaimService
{
    private readonly HttpContext httpContext;

    public UserClaimService(IHttpContextAccessor httpContextAccessor)
    {
        httpContext = httpContextAccessor.HttpContext;
    }

    public string GetApplicationId()
    {
        return httpContext.User.GetApplicationId();
    }

    public Guid GetId()
    {
        return httpContext.User.GetId();
    }

    public string GetUserName()
    {
        return httpContext.User.GetUserName();
    }

    public Guid GetTenantId() => Guid.Empty;

    public ClaimsIdentity GetIdentity()
    {
        var identity = httpContext.User.Identity;
        return identity as ClaimsIdentity;
    }
}