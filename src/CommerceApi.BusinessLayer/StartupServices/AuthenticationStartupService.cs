using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommerceApi.BusinessLayer.StartupServices;

public class AuthenticationStartupService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public AuthenticationStartupService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var roleNames = new string[] { RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User };
        await identityService.CreateRolesAsync(roleNames);

        var administratorUserSection = _configuration.GetSection("AdministratorUser");
        var powerUserSection = _configuration.GetSection("PowerUser");

        var administratorUser = new ApplicationUser
        {
            FirstName = administratorUserSection["FirstName"],
            UserName = administratorUserSection["Email"],
            Email = administratorUserSection["Email"]
        };

        var powerUser = new ApplicationUser
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