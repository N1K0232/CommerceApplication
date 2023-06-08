using System.Security.Claims;
using CommerceApi.Authentication.Extensions;
using CommerceApi.SharedServices;
using Microsoft.AspNetCore.Http;

namespace CommerceApi.Authentication.InternalServices;

internal class UserClaimService : IUserClaimService
{
    private readonly HttpContext _httpContext;

    public UserClaimService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContext = httpContextAccessor.HttpContext;
    }

    public string GetApplicationId()
    {
        return _httpContext.User.GetApplicationId();
    }

    public Guid GetId()
    {
        return _httpContext.User.GetId();
    }

    public string GetUserName()
    {
        return _httpContext.User.GetUserName();
    }

    public Guid GetTenantId() => Guid.Empty;

    public ClaimsIdentity GetIdentity()
    {
        var identity = _httpContext.User.Identity;
        return identity as ClaimsIdentity;
    }
}