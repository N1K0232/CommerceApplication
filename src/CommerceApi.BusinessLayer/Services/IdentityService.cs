using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.Authentication.Extensions;
using CommerceApi.Authentication.Managers;
using CommerceApi.Authentication.Settings;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CommerceApi.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationRoleManager _roleManager;
    private readonly ApplicationSignInManager _signInManager;

    private readonly EmailTokenProvider<ApplicationUser> _emailTokenProvider;
    private readonly PhoneNumberTokenProvider<ApplicationUser> _phoneNumberTokenProvider;

    public IdentityService(IOptions<JwtSettings> jwtSettingsOptions,
                           ApplicationUserManager userManager,
                           ApplicationRoleManager roleManager,
                           ApplicationSignInManager signInManager,
                           EmailTokenProvider<ApplicationUser> emailTokenProvider,
                           PhoneNumberTokenProvider<ApplicationUser> phoneNumberTokenProvider)
    {
        _jwtSettings = jwtSettingsOptions.Value;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _emailTokenProvider = emailTokenProvider;
        _phoneNumberTokenProvider = phoneNumberTokenProvider;
    }

    public async Task CreateRoleAsync(string roleName)
    {
        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            var role = new ApplicationRole(roleName);
            await _roleManager.CreateAsync(role);
        }
    }

    public async Task CreateRolesAsync(IEnumerable<string> roleNames)
    {
        foreach (var roleName in roleNames)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var role = new ApplicationRole(roleName);
                await _roleManager.CreateAsync(role);
            }
        }
    }

    public async Task<IdentityResult> DeleteAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var userRoles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);

        if (userRoles.Any(r => r.Equals(RoleNames.Administrator) || r.Equals(RoleNames.PowerUser)))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "400",
                Description = "You can't delete an administrator or a power user"
            });
        }

        var deletedClaimsResult = await _userManager.RemoveClaimsAsync(user, userClaims);
        var deletedRolesResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
        var deletedUserResult = await _userManager.DeleteAsync(user);

        var success = deletedClaimsResult.Succeeded && deletedRolesResult.Succeeded && deletedUserResult.Succeeded;
        if (!success)
        {
            var errors = GetIdentityErrors(deletedClaimsResult, deletedRolesResult, deletedUserResult);
            return IdentityResult.Failed(errors.ToArray());
        }

        return IdentityResult.Success;
    }

    public Task<string> GenerateTwoFactorTokenAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<SignInResult> TwoFactorLoginAsync(string token)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicationUser> GetUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user;
    }

    public async Task<LoginResponse> LoginAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        await _userManager.UpdateSecurityStampAsync(user);
        await _userManager.GenerateConcurrencyStampAsync(user);

        var claims = await GetClaimsAsync(user);
        var loginResponse = CreateLoginResponse(claims);

        await SaveRefreshTokenAsync(user, loginResponse.RefreshToken);
        return loginResponse;
    }

    public async Task<LoginResponse> RefreshTokenAsync(ClaimsPrincipal principal, string refreshToken)
    {
        var userId = principal.GetId();
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user?.RefreshToken is null || user?.RefreshTokenExpirationDate < DateTime.UtcNow || user.RefreshToken != refreshToken)
        {
            return null;
        }

        var loginResponse = CreateLoginResponse(principal.Claims);
        await SaveRefreshTokenAsync(user, loginResponse.RefreshToken);

        return loginResponse;
    }

    public async Task<IdentityResult> RegisterUserAsync(ApplicationUser user, Address address, string password, string role)
    {
        var identityResult = await RegisterAsync(user, password);
        if (identityResult.Succeeded)
        {
            await _userManager.AddAddressAsync(user, address);
            identityResult = await _userManager.AddToRoleAsync(user, role);
        }

        return identityResult;
    }

    public async Task<IdentityResult> RegisterAsync(ApplicationUser user, string password, params string[] roles)
    {
        var identityResult = await RegisterAsync(user, password);
        if (identityResult.Succeeded)
        {
            identityResult = await _userManager.AddToRolesAsync(user, roles);
        }

        return identityResult;
    }

    public async Task<SignInResult> SignInAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return await _signInManager.PasswordSignInAsync(user, password, false, true);
    }

    public async Task SignOutAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            await _signInManager.SignOutAsync();
        }
    }

    public Task<ClaimsPrincipal> ValidateAccessTokenAsync(string accessToken)
    {
        var securityKeyBytes = Encoding.UTF8.GetBytes(_jwtSettings.SecurityKey);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(securityKeyBytes),
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var user = tokenHandler.ValidateToken(accessToken, parameters, out var securityToken);
            if (securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                return Task.FromResult(user);
            }
        }
        catch
        {
        }

        return Task.FromResult<ClaimsPrincipal>(null);
    }

    public async Task<IdentityResult> SetLockoutEnabledAsync(string email, DateTimeOffset lockoutEnd, bool enabled)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            var error = new IdentityError { Code = "404", Description = "user not found" };
            return IdentityResult.Failed(error);
        }

        var setLockoutEnabledResult = await _userManager.SetLockoutEnabledAsync(user, enabled);
        var setLockoutEndDateResult = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

        if (setLockoutEnabledResult.Succeeded && setLockoutEndDateResult.Succeeded)
        {
            return IdentityResult.Success;
        }

        var errors = GetIdentityErrors(setLockoutEnabledResult, setLockoutEndDateResult);
        return IdentityResult.Failed(errors.ToArray());
    }

    private static IEnumerable<IdentityError> GetIdentityErrors(params IdentityResult[] identityResults)
    {
        var errors = new List<IdentityError>();
        foreach (var result in identityResults)
        {
            foreach (var error in result.Errors)
            {
                errors.Add(error);
            }
        }

        return errors;
    }

    private LoginResponse CreateLoginResponse(IEnumerable<Claim> claims)
    {
        var securityKeyBytes = Encoding.UTF8.GetBytes(_jwtSettings.SecurityKey);
        var securityKey = new SymmetricSecurityKey(securityKeyBytes);

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(_jwtSettings.Issuer, _jwtSettings.Audience, claims,
            DateTime.UtcNow, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes), credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(jwtSecurityToken);

        using var generator = RandomNumberGenerator.Create();
        var randomNumber = new byte[256];

        generator.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);

        var loginResponse = new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken };
        return loginResponse;
    }

    private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var userAddresses = await _userManager.GetAddressesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);

        var hasItems = userClaims?.Any() ?? false;
        if (!hasItems)
        {
            userClaims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            foreach (var role in userRoles)
            {
                userClaims.Add(new(ClaimTypes.Role, role));
            }

            await _userManager.AddClaimsAsync(user, userClaims);
        }

        var claims = new List<Claim>(userClaims)
        {
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(ClaimTypes.DateOfBirth, user.DateOfBirth?.ToString() ?? string.Empty),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp),
            new Claim(ClaimTypes.GroupSid, user.ConcurrencyStamp),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
        };

        hasItems = userAddresses?.Any() ?? false;
        if (hasItems)
        {
            foreach (var address in userAddresses)
            {
                claims.Add(new(ClaimTypes.StreetAddress, address.Street));
                claims.Add(new(CustomClaimTypes.City, address.City));
                claims.Add(new(ClaimTypes.PostalCode, address.PostalCode));
                claims.Add(new(ClaimTypes.Country, address.Country));
            }
        }

        return claims;
    }

    private async Task UpdateClaimAsync(ApplicationUser user, string type, string value)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var claim = userClaims.FirstOrDefault(c => c.Type == type);

        await _userManager.ReplaceClaimAsync(user, claim, new Claim(type, value));
    }

    private async Task<IdentityResult> RegisterAsync(ApplicationUser user, string password)
    {
        var identityResult = await _userManager.CreateAsync(user, password);
        return identityResult;
    }

    private async Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationMinutes);

        await _userManager.UpdateAsync(user);
    }
}