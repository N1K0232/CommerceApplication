using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommerceApi.BusinessLayer.StartupServices;

public class AuthenticationStartupService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;

    public AuthenticationStartupService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var roleNames = new string[] { RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User };
        await identityService.CreateRolesAsync(roleNames);

        var administratorUserSection = configuration.GetSection("AdministratorUser");
        var powerUserSection = configuration.GetSection("PowerUser");

        var administratorUser = new AuthenticationUser
        {
            FirstName = administratorUserSection["FirstName"],
            UserName = administratorUserSection["Email"],
            Email = administratorUserSection["Email"]
        };

        var powerUser = new AuthenticationUser
        {
            FirstName = powerUserSection["FirstName"],
            UserName = powerUserSection["Email"],
            Email = powerUserSection["Email"]
        };

        await identityService.RegisterAsync(administratorUser, administratorUserSection["Password"], RoleNames.Administrator, RoleNames.User);
        await identityService.RegisterAsync(powerUser, powerUserSection["Password"], RoleNames.PowerUser, RoleNames.User);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}