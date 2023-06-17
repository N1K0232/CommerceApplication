using CommerceApi.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CommerceApi.Authentication.Managers;

public class ApplicationRoleManager : RoleManager<ApplicationRole>
{
    private CancellationTokenSource _tokenSource = new CancellationTokenSource();

    public ApplicationRoleManager(IRoleStore<ApplicationRole> store,
        IEnumerable<IRoleValidator<ApplicationRole>> roleValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        ILogger<ApplicationRoleManager> logger)
        : base(store, roleValidators, keyNormalizer, errors, logger)
    {
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

    public override Task<IdentityResult> CreateAsync(ApplicationRole role)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        role.ConcurrencyStamp = Guid.NewGuid().ToString();
        return base.CreateAsync(role);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tokenSource.Dispose();
            _tokenSource = null;
        }

        base.Dispose(disposing);
    }
}