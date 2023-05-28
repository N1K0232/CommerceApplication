using System.IdentityModel.Tokens.Jwt;
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
    private readonly JwtSettings jwtSettings;
    private readonly ApplicationUserManager userManager;
    private readonly RoleManager<ApplicationRole> roleManager;
    private readonly ApplicationSignInManager signInManager;

    public IdentityService(IOptions<JwtSettings> jwtSettingsOptions,
                           ApplicationUserManager userManager,
                           RoleManager<ApplicationRole> roleManager,
                           ApplicationSignInManager signInManager)
    {
        jwtSettings = jwtSettingsOptions.Value;

        this.userManager = userManager;
        this.roleManager = roleManager;
        this.signInManager = signInManager;
    }

    public async Task CreateRoleAsync(string roleName)
    {
        await CreateRoleCoreAsync(roleName);
    }

    public async Task CreateRolesAsync(IEnumerable<string> roleNames)
    {
        foreach (var roleName in roleNames)
        {
            await CreateRoleCoreAsync(roleName);
        }
    }

    public async Task<IdentityResult> DeleteAccountAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        var userRoles = await userManager.GetRolesAsync(user);
        var userClaims = await userManager.GetClaimsAsync(user);

        if (userRoles.Any(r => r.Equals(RoleNames.Administrator) || r.Equals(RoleNames.PowerUser)))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "400",
                Description = "You can't delete an administrator or a power user"
            });
        }

        await userManager.RemoveClaimsAsync(user, userClaims);
        await userManager.RemoveFromRolesAsync(user, userRoles);

        var identityResult = await userManager.DeleteAsync(user);
        return identityResult;
    }

    public async Task<ApplicationUser> GetUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user;
    }

    public async Task<LoginResponse> LoginAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        await userManager.UpdateSecurityStampAsync(user);
        await userManager.GenerateConcurrencyStampAsync(user);

        var claims = await GetClaimsAsync(user);

        var loginResponse = CreateLoginResponse(claims);
        await SaveRefreshTokenAsync(user, loginResponse.RefreshToken);

        return loginResponse;
    }

    public async Task<LoginResponse> RefreshTokenAsync(ClaimsPrincipal principal, string refreshToken)
    {
        var userId = principal.GetId();
        var user = await userManager.FindByIdAsync(userId.ToString());

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
            await userManager.AddAddressAsync(user, address);
            identityResult = await userManager.AddToRoleAsync(user, role);
        }

        return identityResult;
    }

    public async Task<IdentityResult> RegisterAsync(ApplicationUser user, string password, params string[] roles)
    {
        var identityResult = await RegisterAsync(user, password);
        if (identityResult.Succeeded)
        {
            identityResult = await userManager.AddToRolesAsync(user, roles);
        }

        return identityResult;
    }

    public async Task<SignInResult> SignInAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);

        var signInResult = await signInManager.PasswordSignInAsync(user, password, false, false);
        return signInResult;
    }

    public async Task SignOutAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            await signInManager.SignOutAsync();
        }
    }

    public Task<ClaimsPrincipal> ValidateAccessTokenAsync(string accessToken)
    {
        var securityKeyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecurityKey);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
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

    private LoginResponse CreateLoginResponse(IEnumerable<Claim> claims)
    {
        var securityKeyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecurityKey);
        var securityKey = new SymmetricSecurityKey(securityKeyBytes);

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(jwtSettings.Issuer, jwtSettings.Audience, claims,
            DateTime.UtcNow, DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes), credentials);

        var tokenHandler = new JwtSecurityTokenHandler();

        var accessToken = tokenHandler.WriteToken(jwtSecurityToken);

        using var generator = RandomNumberGenerator.Create();
        var randomNumber = new byte[256];
        generator.GetBytes(randomNumber);

        var refreshToken = Convert.ToBase64String(randomNumber);

        var loginResponse = new LoginResponse(accessToken, refreshToken);
        return loginResponse;
    }

    private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user)
    {
        var userRoles = await userManager.GetRolesAsync(user);
        var userAddresses = await userManager.GetAddressesAsync(user);
        var userClaims = await userManager.GetClaimsAsync(user);

        if (userClaims is null || !userClaims.Any())
        {
            userClaims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            foreach (var role in userRoles)
            {
                userClaims.Add(new(ClaimTypes.Role, role));
            }

            await userManager.AddClaimsAsync(user, userClaims);
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

        if (userAddresses != null && userAddresses.Any())
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

    private async Task CreateRoleCoreAsync(string roleName)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            var role = new ApplicationRole(roleName)
            {
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            await roleManager.CreateAsync(role);
        }
    }

    private async Task UpdateClaimAsync(ApplicationUser user, string type, string value)
    {
        var userClaims = await userManager.GetClaimsAsync(user);
        var claim = userClaims.FirstOrDefault(c => c.Type == type);

        await userManager.ReplaceClaimAsync(user, claim, new Claim(type, value));
    }

    private async Task<IdentityResult> RegisterAsync(ApplicationUser user, string password)
    {
        var identityResult = await userManager.CreateAsync(user, password);
        return identityResult;
    }

    private async Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshTokenExpirationMinutes);
        await userManager.UpdateAsync(user);
    }
}