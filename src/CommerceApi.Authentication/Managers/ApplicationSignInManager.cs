using CommerceApi.Authentication.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommerceApi.Authentication.Managers;

public class ApplicationSignInManager : SignInManager<ApplicationUser>
{
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

    public async Task<SignInResult> SignInAsync(string email, string password)
    {
        var user = await UserManager.FindByEmailAsync(email);
        if (user == null)
        {
            return SignInResult.Failed;
        }

        return await PasswordSignInAsync(user, password, false, false);
    }

    public override async Task<SignInResult> PasswordSignInAsync(ApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure)
    {
        var canSignIn = await CanSignInAsync(user);
        if (!canSignIn)
        {
            return SignInResult.Failed;
        }

        var signInResult = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        if (!signInResult.Succeeded)
        {
            user.AccessFailedCount++;
            await UserManager.UpdateAsync(user);
        }

        return signInResult;
    }
}