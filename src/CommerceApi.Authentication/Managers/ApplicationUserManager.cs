using CommerceApi.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommerceApi.Authentication.Managers;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    public ApplicationUserManager(IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<ApplicationUserManager> logger)
        : base(store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger)
    {
    }

    public override async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        var userExists = await Users.AnyAsync(u => u.NormalizedEmail == user.NormalizedEmail || u.NormalizedUserName == user.NormalizedUserName);
        if (!userExists)
        {
            return await base.CreateAsync(user, password);
        }

        return IdentityResult.Failed(new IdentityError
        {
            Code = "409",
            Description = "a user with the same email or username already exists"
        });
    }

    public override async Task<IdentityResult> DeleteAsync(ApplicationUser user)
    {
        var userRoles = await GetRolesAsync(user);
        var userClaims = await GetClaimsAsync(user);

        await RemoveFromRolesAsync(user, userRoles);
        await RemoveClaimsAsync(user, userClaims);
        return await base.DeleteAsync(user);
    }

    public override async Task<string> GenerateConcurrencyStampAsync(ApplicationUser user)
    {
        user.ConcurrencyStamp = await base.GenerateConcurrencyStampAsync(user);
        return user.ConcurrencyStamp;
    }
}