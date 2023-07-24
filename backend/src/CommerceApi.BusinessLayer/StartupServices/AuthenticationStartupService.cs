using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.Authentication.Managers;
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
        var roleManager = scope.ServiceProvider.GetRequiredService<ApplicationRoleManager>();
        var userManager = scope.ServiceProvider.GetRequiredService<ApplicationUserManager>();


        var roleNames = new string[] { RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User, RoleNames.Customer };
        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

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

        var administratorUserResult = await userManager.CreateAsync(administratorUser, administratorUserSection["Password"]);
        if (administratorUserResult.Succeeded)
        {
            await userManager.AddToRolesAsync(administratorUser, new List<string> { RoleNames.Administrator, RoleNames.User });
        }

        var powerUserResult = await userManager.CreateAsync(powerUser, powerUserSection["Password"]);
        if (powerUserResult.Succeeded)
        {
            await userManager.AddToRolesAsync(powerUser, new List<string> { RoleNames.PowerUser, RoleNames.User });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}