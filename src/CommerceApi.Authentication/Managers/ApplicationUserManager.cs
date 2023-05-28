using CommerceApi.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommerceApi.Authentication.Managers;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    private readonly AuthenticationDataContext authenticationDataContext;

    public ApplicationUserManager(AuthenticationDataContext authenticationDataContext,
        IUserStore<ApplicationUser> store,
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
        this.authenticationDataContext = authenticationDataContext;
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

    public async Task<IdentityResult> AddAddressAsync(ApplicationUser user, Address address)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(address, nameof(address));

        address.UserId = user.Id;

        await authenticationDataContext.Addresses.AddAsync(address);
        await authenticationDataContext.SaveChangesAsync();

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> AddAddressesAsync(ApplicationUser user, IEnumerable<Address> addresses)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var hasItems = addresses?.Any() ?? false;
        if (!hasItems)
        {
            throw new ArgumentNullException(nameof(addresses), "you must provide at least one member");
        }

        foreach (var address in addresses)
        {
            address.UserId = user.Id;
        }

        await authenticationDataContext.AddRangeAsync(addresses);
        await authenticationDataContext.SaveChangesAsync();

        return IdentityResult.Success;
    }

    public async Task<IEnumerable<Address>> GetAddressesAsync(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(ApplicationUser));

        var query = authenticationDataContext.Addresses.AsQueryable();
        var addresses = await query.Where(a => a.UserId == user.Id).ToListAsync();
        return addresses;
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