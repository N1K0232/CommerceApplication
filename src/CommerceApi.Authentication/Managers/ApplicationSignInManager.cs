using CommerceApi.Authentication.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommerceApi.Authentication.Managers;

public class ApplicationSignInManager : SignInManager<ApplicationUser>
{
    private CancellationTokenSource _tokenSource = new CancellationTokenSource();

    public ApplicationSignInManager(ApplicationUserManager userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<ApplicationSignInManager> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation)
        : base(userManager,
            contextAccessor,
            claimsFactory,
            optionsAccessor,
            logger,
            schemes,
            confirmation)
    {
    }

    public async Task<SignInResult> SignInAsync(string email, string password, bool isPersistent, bool lockoutOnFailure)
    {
        var user = await UserManager.FindByEmailAsync(email).ConfigureAwait(false);
        return await ExecuteSignInAsync(user, password, isPersistent, lockoutOnFailure).ConfigureAwait(false);
    }

    public override async Task<SignInResult> PasswordSignInAsync(ApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure)
    {
        return await ExecuteSignInAsync(user, password, isPersistent, lockoutOnFailure);
    }

    private async Task<SignInResult> ExecuteSignInAsync(ApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure)
    {
        var canSignIn = await CanSignInAsync(user).ConfigureAwait(false);
        if (!canSignIn)
        {
            return SignInResult.Failed;
        }

        return await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure).ConfigureAwait(false);
    }
}