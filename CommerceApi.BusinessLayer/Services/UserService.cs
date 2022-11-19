using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.Authentication.Extensions;
using CommerceApi.Authentication.Settings;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Requests;
using CommerceApi.Shared.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CommerceApi.BusinessLayer.Services;

public class UserService : IUserService
{
    private readonly JwtSettings jwtSettings;

    private readonly UserManager<AuthenticationUser> userManager;
    private readonly SignInManager<AuthenticationUser> signInManager;

    private readonly ILogger<UserService> logger;
    private readonly IMemoryCache cache;

    private readonly IMapper mapper;

    public UserService(IOptions<JwtSettings> jwtSettingsOptions,
        UserManager<AuthenticationUser> userManager,
        SignInManager<AuthenticationUser> signInManager,
        ILogger<UserService> logger,
        IMemoryCache cache,
        IMapper mapper)
    {
        jwtSettings = jwtSettingsOptions.Value;

        this.userManager = userManager;
        this.signInManager = signInManager;

        this.logger = logger;
        this.cache = cache;

        this.mapper = mapper;
    }


    public async Task<RegisterResponse> DeleteAccountAsync(Guid userId)
    {
        logger.LogInformation("deleting account");

        var user = await GetUserAsync(userId);
        if (user is null)
        {
            return new RegisterResponse(false, new List<string> { "User not found" });
        }

        var roles = await GetRolesAsync(user);
        if (roles.Any(r => r.Equals(RoleNames.Administrator)))
        {
            return new RegisterResponse(false, new List<string> { "can't delete an administrator" });
        }

        var result = await userManager.DeleteAsync(user);
        return new RegisterResponse(result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        logger.LogInformation("user login", request);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return null;
        }

        var signInResult = await signInManager.PasswordSignInAsync(user, request.Password, false, false);
        if (!signInResult.Succeeded)
        {
            user.AccessFailedCount++;
            await userManager.UpdateAsync(user);

            return null;
        }

        await userManager.UpdateSecurityStampAsync(user);

        var roles = await GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.DateOfBirth, user.DateOfBirth.ToString()),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        }.Union(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var loginResponse = CreateResponse(claims);
        await SaveRefreshTokenAsync(user, loginResponse.RefreshToken);

        return loginResponse;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        logger.LogInformation("user registration", request);

        var user = await CreateUserAsync(request);

        var userExists = await userManager.Users.AnyAsync(u => u.Id == user.Id);
        if (userExists)
        {
            var result = await userManager.UpdateAsync(user);
            return new RegisterResponse(result.Succeeded, result.Errors.Select(e => e.Description));
        }
        else
        {
            var result = await userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                result = await userManager.AddToRoleAsync(user, RoleNames.User);
            }

            return new RegisterResponse(result.Succeeded, result.Errors.Select(e => e.Description));
        }
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        logger.LogInformation("user refresh token", request);

        var user = ValidateAccessToken(request.AccessToken);
        if (user is not null)
        {
            var userId = user.GetId();
            var dbUser = await GetUserAsync(userId);

            if (dbUser?.RefreshToken is null || dbUser?.RefreshTokenExpirationDate < DateTime.UtcNow || dbUser?.RefreshToken != request.RefreshToken)
            {
                return null;
            }

            var loginResponse = CreateResponse(user.Claims);
            await SaveRefreshTokenAsync(dbUser, loginResponse.RefreshToken);

            return loginResponse;
        }

        return null;
    }

    private async Task<AuthenticationUser> CreateUserAsync(RegisterRequest request)
    {
        var userId = request.Id.GetValueOrDefault(Guid.Empty);
        var user = await GetUserAsync(userId);

        if (user is null)
        {
            user = mapper.Map<AuthenticationUser>(request);
        }
        else
        {
            mapper.Map(request, user);
        }

        return user;
    }

    private async Task<AuthenticationUser> GetUserAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        return user;
    }

    private async Task<IEnumerable<string>> GetRolesAsync(AuthenticationUser user)
    {
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return roles;
    }

    private async Task SaveRefreshTokenAsync(AuthenticationUser user, string refreshToken)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes);

        await userManager.UpdateAsync(user);
    }

    private LoginResponse CreateResponse(IEnumerable<Claim> claims)
    {
        var securityKey = GetSecurityKey();
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(jwtSettings.Issuer, jwtSettings.Audience, claims,
            DateTime.UtcNow, DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes), credentials);

        using var generator = RandomNumberGenerator.Create();
        var randomNumber = new byte[256];
        generator.GetBytes(randomNumber);

        var tokenHandler = new JwtSecurityTokenHandler();

        var accessToken = tokenHandler.WriteToken(jwtSecurityToken);
        var refreshToken = Convert.ToBase64String(randomNumber);

        var loginResponse = new LoginResponse(accessToken, refreshToken);
        return loginResponse;
    }

    private ClaimsPrincipal ValidateAccessToken(string accessToken)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var user = tokenHandler.ValidateToken(accessToken, parameters, out var securityToken);
            if (securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                return user;
            }
        }
        catch
        {
        }

        return null;
    }

    private SymmetricSecurityKey GetSecurityKey()
    {
        var bytes = Encoding.UTF8.GetBytes(jwtSettings.SecurityKey);
        return new SymmetricSecurityKey(bytes);
    }
}