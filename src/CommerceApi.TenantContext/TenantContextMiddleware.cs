using Microsoft.AspNetCore.Http;

namespace CommerceApi.TenantContext;

internal class TenantContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly TenantContextOptions _tenantContextOptions;

    public TenantContextMiddleware(RequestDelegate next, ITenantContextAccessor tenantContextAccessor, TenantContextOptions tenantContextOptions)
    {
        _next = next;
        _tenantContextAccessor = tenantContextAccessor;
        _tenantContextOptions = tenantContextOptions;
    }

    public async Task Invoke(HttpContext context)
    {
        _tenantContextAccessor.TenantContext = new DefaultTenantContext();

        var host = context.Request.Host.Host;
        var tenants = host?.Split('.') ?? Array.Empty<string>();
        var tenant = tenants[0]?.ToLowerInvariant()?.Trim() ?? string.Empty;

        if (_tenantContextOptions.AvailableTenants.Contains(tenant))
        {
            _tenantContextAccessor.TenantContext.Name = tenant;
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status404NotFound;
    }
}