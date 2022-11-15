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

    public Guid GetId() => httpContext.User.GetId();

    public string GetUserName() => httpContext.User.GetUserName();
}