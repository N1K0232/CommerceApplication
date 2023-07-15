using CommerceApi.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace CommerceApi.Authorization.Handlers;

public class TenantHandler : AuthorizationHandler<TenantRequirement>
{
    public TenantHandler()
    {
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantRequirement requirement)
    {
        return Task.CompletedTask;
    }
}