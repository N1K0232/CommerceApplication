using System.Security.Claims;
using CommerceApi.Authentication.Extensions;
using CommerceApi.SharedServices;
using Microsoft.AspNetCore.Http;

namespace CommerceApi.Authentication.InternalServices;

internal class UserClaimService : IUserClaimService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserClaimService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext HttpContext
    {
        get
        {
            return _httpContextAccessor.HttpContext;
        }
    }

    public string GetApplicationId() => HttpContext.User.GetApplicationId();

    public Guid GetId() => HttpContext.User.GetId();

    public ClaimsIdentity GetIdentity()
    {
        var identity = HttpContext.User.Identity;
        return identity as ClaimsIdentity;
    }

    public Guid GetTenantId()
    {
        var tenantIdString = GetClaimValue("TenantId");
        return Guid.TryParse(tenantIdString, out var tenantId) ? tenantId : Guid.Empty;
    }

    public string GetUserName() => HttpContext.User.GetUserName();

    private string GetClaimValue(string claimType)
        => HttpContext.User.GetClaimValueInternal(claimType);
}