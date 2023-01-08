﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.Authentication.Extensions;
using CommerceApi.Authentication.Settings;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.BusinessLayer.Settings;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using CommerceApi.Shared.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services;

public class UserService : IUserService
{
    private const string AuthenticatedUser = nameof(AuthenticatedUser);

    private readonly JwtSettings jwtSettings;
    private readonly AppSettings appSettings;

    private UserManager<AuthenticationUser> userManager;
    private RoleManager<AuthenticationRole> roleManager;
    private SignInManager<AuthenticationUser> signInManager;

    private readonly IValidator<LoginRequest> loginValidator;
    private readonly IValidator<RegisterRequest> registerValidator;
    private readonly IValidator<RefreshTokenRequest> refreshTokenValidator;

    private readonly ILogger<UserService> logger;
    private readonly IMemoryCache cache;
    private readonly IMapper mapper;

    private bool disposed = false;


    public UserService(IOptions<JwtSettings> jwtSettingsOptions,
        IOptions<AppSettings> appSettingsOptions,
        UserManager<AuthenticationUser> userManager,
        RoleManager<AuthenticationRole> roleManager,
        SignInManager<AuthenticationUser> signInManager,
        IValidator<LoginRequest> loginValidator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<RefreshTokenRequest> refreshTokenValidator,
        ILogger<UserService> logger,
        IMemoryCache cache,
        IMapper mapper)
    {
        jwtSettings = jwtSettingsOptions.Value;
        appSettings = appSettingsOptions.Value;

        this.userManager = userManager;
        this.roleManager = roleManager;
        this.signInManager = signInManager;

        this.loginValidator = loginValidator;
        this.registerValidator = registerValidator;
        this.refreshTokenValidator = refreshTokenValidator;

        this.logger = logger;
        this.cache = cache;

        this.mapper = mapper;
    }


    ~UserService()
    {
        Dispose(disposing: false);
    }


    public async Task<RegisterResponse> DeleteAccountAsync(Guid userId)
    {
        ThrowIfDisposed();

        logger.LogInformation("deleting account");

        var user = await GetUserAsync(userId);
        if (user is null)
        {
            return new RegisterResponse
            {
                Succeeded = false,
                Errors = new List<string> { "User not found" }
            };
        }

        var roles = await GetRolesAsync(user);
        if (roles.Any(r => r.Equals(RoleNames.Administrator)))
        {
            return new RegisterResponse
            {
                Succeeded = false,
                Errors = new List<string> { "can't delete an administrator" }
            };
        }

        var result = await userManager.DeleteAsync(user);

        return new RegisterResponse
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.Select(e => e.Description)
        };
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        ThrowIfDisposed();

        logger.LogInformation("user login", request);

        var validationResult = await loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.ClientError, "error occurred during login", validationErrors);
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Fail(FailureReasons.GenericError, "wrong email");
        }

        var signInResult = await signInManager.PasswordSignInAsync(user, request.Password, false, false);
        if (!signInResult.Succeeded)
        {
            user.AccessFailedCount++;
            await userManager.UpdateAsync(user);

            return Result.Fail(FailureReasons.ClientError, "wrong password");
        }

        SaveAuthenticateUser(mapper.Map<User>(user));

        await userManager.UpdateSecurityStampAsync(user);

        var roles = await GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(CustomClaimTypes.ApplicationId, appSettings.ApplicationId),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.DateOfBirth, user.DateOfBirth.ToString()),
            new Claim(CustomClaimTypes.Age, Convert.ToInt32((DateTime.UtcNow.Date - user.DateOfBirth.Date).TotalDays / 365).ToString()),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
        }.Union(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var loginResponse = CreateResponse(claims);
        await SaveRefreshTokenAsync(user, loginResponse.RefreshToken);

        return loginResponse;
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        ThrowIfDisposed();

        logger.LogInformation("user registration", request);

        var validationResult = await registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
        }

        var user = new AuthenticationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            UserName = request.UserName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(user, RoleNames.User);
        }

        var registerResponse = new RegisterResponse
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.Select(e => e.Description)
        };

        return registerResponse;
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        ThrowIfDisposed();

        logger.LogInformation("user refresh token", request);

        var validationResult = await refreshTokenValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.GenericError, "errors during refresh token", validationErrors);
        }

        var user = ValidateAccessToken(request.AccessToken);
        if (user is not null)
        {
            var userId = user.GetId();
            var dbUser = await GetUserAsync(userId);

            if (dbUser?.RefreshToken is null || dbUser?.RefreshTokenExpirationDate < DateTime.UtcNow || dbUser?.RefreshToken != request.RefreshToken)
            {
                return null;
            }

            SaveAuthenticateUser(mapper.Map<User>(dbUser));

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

        var tokenHandler = new JwtSecurityTokenHandler();

        var accessToken = tokenHandler.WriteToken(jwtSecurityToken);

        using var generator = RandomNumberGenerator.Create();
        var randomNumber = new byte[256];
        generator.GetBytes(randomNumber);

        var refreshToken = Convert.ToBase64String(randomNumber);

        var loginResponse = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

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
            IssuerSigningKey = GetSecurityKey(),
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

        var securityKey = new SymmetricSecurityKey(bytes);
        return securityKey;
    }

    private void SaveAuthenticateUser(User authenticatedUser)
    {
        var found = cache.TryGetValue<User>(AuthenticatedUser, out var user);
        if (found && user is not null)
        {
            cache.Remove(AuthenticatedUser);
        }

        var entry = cache.CreateEntry(AuthenticatedUser);

        entry.Value = authenticatedUser;
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(jwtSettings.ExpirationMinutes);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            userManager.Dispose();
            userManager = null;

            roleManager.Dispose();
            roleManager = null;

            signInManager = null;

            disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}