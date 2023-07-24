using System.Linq.Dynamic.Core;
using System.Security.Claims;
using AutoMapper;
using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.Authentication.Extensions;
using CommerceApi.Authentication.Managers;
using CommerceApi.Authentication.RemoteServices.Interfaces;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.Shared.Models.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private readonly IJwtBearerTokenGeneratorService _jwtBearerTokenGeneratorService;

    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationRoleManager _roleManager;
    private readonly ApplicationSignInManager _signInManager;

    private readonly EmailTokenProvider<ApplicationUser> _emailTokenProvider;
    private readonly PhoneNumberTokenProvider<ApplicationUser> _phoneNumberTokenProvider;

    private readonly IMapper _mapper;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenValidator;
    private readonly IValidator<SaveAddressRequest> _addressValidator;

    public IdentityService(IJwtBearerTokenGeneratorService jwtBearerTokenGeneratorService,
                           ApplicationUserManager userManager,
                           ApplicationRoleManager roleManager,
                           ApplicationSignInManager signInManager,
                           EmailTokenProvider<ApplicationUser> emailTokenProvider,
                           PhoneNumberTokenProvider<ApplicationUser> phoneNumberTokenProvider,
                           IMapper mapper,
                           IValidator<LoginRequest> loginValidator,
                           IValidator<RegisterRequest> registerValidator,
                           IValidator<RefreshTokenRequest> refreshTokenValidator,
                           IValidator<SaveAddressRequest> addressValidator)
    {
        _jwtBearerTokenGeneratorService = jwtBearerTokenGeneratorService;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _emailTokenProvider = emailTokenProvider;
        _phoneNumberTokenProvider = phoneNumberTokenProvider;
        _mapper = mapper;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _addressValidator = addressValidator;
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

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest loginRequest)
    {
        var validationResult = await _loginValidator.ValidateAsync(loginRequest);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>(validationResult.Errors.Count);
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.ClientError, "invalid request", validationErrors);
        }

        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No account associated to the specified email found");
        }

        var signInResult = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, false, false);
        if (!signInResult.Succeeded)
        {
            return Result.Fail(FailureReasons.DatabaseError, "invalid password");
        }

        var claims = await GetClaimsAsync(user);
        var accessToken = _jwtBearerTokenGeneratorService.GenerateAccessToken(claims);
        var refreshToken = _jwtBearerTokenGeneratorService.GenerateRefreshToken();

        var loginResponse = new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken };

        await SaveRefreshTokenAsync(user, loginResponse.RefreshToken);
        return loginResponse;
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(refreshTokenRequest);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>(validationResult.Errors.Count);
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.ClientError, "invalid request", validationErrors);
        }

        var principal = _jwtBearerTokenGeneratorService.ValidateAccessToken(refreshTokenRequest.AccessToken);
        var userId = principal.GetId();
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user?.RefreshToken is null || user?.RefreshTokenExpirationDate < DateTime.UtcNow || user.RefreshToken != refreshTokenRequest.RefreshToken)
        {
            return null;
        }

        var accessToken = _jwtBearerTokenGeneratorService.GenerateAccessToken(principal.Claims);
        var refreshToken = _jwtBearerTokenGeneratorService.GenerateRefreshToken();

        var loginResponse = new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken };
        await SaveRefreshTokenAsync(user, loginResponse.RefreshToken);

        return loginResponse;
    }

    public async Task<Result> RegisterAsync(RegisterRequest registerRequest)
    {
        var validationResult = await _registerValidator.ValidateAsync(registerRequest);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>(validationResult.Errors.Count);
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }
        }

        var user = _mapper.Map<ApplicationUser>(registerRequest);
        var roles = new List<string> { registerRequest.Role, RoleNames.User };

        var registerResult = await _userManager.CreateAsync(user, registerRequest.Password);
        var addRolesResult = await _userManager.AddToRolesAsync(user, roles);

        var errors = GetIdentityErrors(registerResult, addRolesResult);
        if (errors.Any())
        {
            var validationErrors = new List<ValidationError>(errors.Count());
            foreach (var error in errors)
            {
                validationErrors.Add(new(error.Code, error.Description));
            }

            return Result.Fail(FailureReasons.DatabaseError, "couldn't registrate your account", validationErrors);
        }

        return Result.Ok();
    }

    public async Task SignOutAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            await _signInManager.SignOutAsync();
        }
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

    private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);

        var hasClaims = userClaims?.Any() ?? false;
        if (!hasClaims)
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
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp),
            new Claim(ClaimTypes.GroupSid, user.ConcurrencyStamp),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
        };

        return claims;
    }

    private async Task UpdateClaimAsync(ApplicationUser user, string type, string value)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var claim = userClaims.FirstOrDefault(c => c.Type == type);

        await _userManager.ReplaceClaimAsync(user, claim, new Claim(type, value));
    }

    private async Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken)
    {
        var refreshTokenExpirationMinutes = _jwtBearerTokenGeneratorService.JwtSettings.RefreshTokenExpirationMinutes;
        var refreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(refreshTokenExpirationMinutes);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = refreshTokenExpirationDate;
        await _userManager.UpdateAsync(user);
    }
}