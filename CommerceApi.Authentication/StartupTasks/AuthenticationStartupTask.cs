using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.Authentication.Models;
using CommerceApi.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CommerceApi.Authentication.StartupTasks;

public class AuthenticationStartupTask : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly AdministratorUser administratorUser;

    public AuthenticationStartupTask(IServiceProvider serviceProvider, IOptions<AdministratorUser> administratorUserOptions)
    {
        this.serviceProvider = serviceProvider;
        administratorUser = administratorUserOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        using var roleManager = services.GetRequiredService<RoleManager<AuthenticationRole>>();

        var roleNames = new string[] { RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User };

        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new AuthenticationRole(roleName));
            }
        }

        using var userManager = services.GetRequiredService<UserManager<AuthenticationUser>>();

        var user = new AuthenticationUser
        {
            FirstName = administratorUser.FirstName,
            LastName = administratorUser.LastName,
            DateOfBirth = administratorUser.DateOfBirth,
            PhoneNumber = administratorUser.PhoneNumber,
            Email = administratorUser.Email,
            UserName = administratorUser.UserName
        };

        var password = StringHasher.GetString(administratorUser.Password);
        await RegisterAsync(user, password, RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User);

        async Task RegisterAsync(AuthenticationUser adminUser, string password, params string[] roles)
        {
            var dbUser = await userManager.FindByNameAsync(adminUser.UserName);
            if (dbUser == null)
            {
                var result = await userManager.CreateAsync(adminUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, roles);
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}