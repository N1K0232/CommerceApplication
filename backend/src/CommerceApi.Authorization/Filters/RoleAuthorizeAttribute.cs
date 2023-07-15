using Microsoft.AspNetCore.Authorization;

namespace CommerceApi.Authorization.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RoleAuthorizeAttribute : AuthorizeAttribute
{
    public RoleAuthorizeAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
        Policy = "UserActive";
    }
}