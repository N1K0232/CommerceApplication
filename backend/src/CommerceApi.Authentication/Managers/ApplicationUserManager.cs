using CommerceApi.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommerceApi.Authentication.Managers;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    private AuthenticationDbContext _authenticationDataContext;
    private CancellationTokenSource _tokenSource;

    public ApplicationUserManager(AuthenticationDbContext authenticationDataContext,
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
        _authenticationDataContext = authenticationDataContext;
        _tokenSource = new CancellationTokenSource();
    }

    protected override CancellationToken CancellationToken
    {
        get
        {
            var token = base.CancellationToken;
            if (token == CancellationToken.None)
            {
                token = _tokenSource.Token;
            }

            return token;
        }
    }

    public override async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        var userExists = await UserExistsAsync(user).ConfigureAwait(false);
        if (!userExists)
        {
            return await base.CreateAsync(user, password).ConfigureAwait(false);
        }

        var error = new IdentityError { Code = "409", Description = "a user with the same email or username already exist" };
        return IdentityResult.Failed(error);
    }

    private async Task<bool> UserExistsAsync(ApplicationUser user)
    {
        var normalizedEmail = user.NormalizedEmail;
        var normalizedUserName = user.NormalizedUserName;

        var query = Users;

        var exists = await query.AnyAsync(u => u.NormalizedUserName == normalizedUserName).ConfigureAwait(false);
        if (!exists)
        {
            exists = await query.AnyAsync(u => u.NormalizedEmail == normalizedEmail).ConfigureAwait(false);
        }

        return exists;
    }

    public async Task<IdentityResult> AddAddressAsync(ApplicationUser user, Address address)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(address, nameof(address));

        address.UserId = user.Id;

        var addresses = _authenticationDataContext.Addresses;
        await addresses.AddAsync(address).ConfigureAwait(false);
        await _authenticationDataContext.SaveChangesAsync().ConfigureAwait(false);

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

        await _authenticationDataContext.AddRangeAsync(addresses).ConfigureAwait(false);
        await _authenticationDataContext.SaveChangesAsync().ConfigureAwait(false);

        return IdentityResult.Success;
    }

    public async Task<IEnumerable<Address>> GetAddressesAsync(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(ApplicationUser));

        var query = _authenticationDataContext.Addresses.Where(a => a.UserId == user.Id);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    public override async Task<string> GenerateConcurrencyStampAsync(ApplicationUser user)
    {
        var concurrencyStamp = await base.GenerateConcurrencyStampAsync(user).ConfigureAwait(false);
        user.ConcurrencyStamp = concurrencyStamp;

        await UpdateAsync(user).ConfigureAwait(false);
        return concurrencyStamp;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_authenticationDataContext != null)
            {
                _authenticationDataContext.Dispose();
                _authenticationDataContext = null;
            }

            if (_tokenSource != null)
            {
                _tokenSource.Dispose();
                _tokenSource = null;
            }

            base.Dispose(disposing);
        }
    }
}