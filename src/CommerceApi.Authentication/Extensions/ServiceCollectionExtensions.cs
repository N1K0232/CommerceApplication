using CommerceApi.Authentication.InternalServices;
using CommerceApi.SharedServices;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserClaimService(this IServiceCollection services)
    {
        services.AddScoped<IUserClaimService, UserClaimService>();
        return services;
    }
}