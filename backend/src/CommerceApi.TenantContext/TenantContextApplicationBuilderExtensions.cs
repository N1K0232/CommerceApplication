using Microsoft.AspNetCore.Builder;

namespace CommerceApi.TenantContext;

public static class TenantContextApplicationBuilderExtensions
{
    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.ApplicationServices.GetService(typeof(ITenantContextAccessor)) is null)
        {
            throw new InvalidOperationException("Unable to find the required services.");
        }

        app.UseMiddleware<TenantContextMiddleware>();
        return app;
    }
}