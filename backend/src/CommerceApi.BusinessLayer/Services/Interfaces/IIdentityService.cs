using CommerceApi.Authentication.Entities;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.Shared.Models.Responses;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IIdentityService
{
    Task CreateRoleAsync(string roleName);

    Task CreateRolesAsync(IEnumerable<string> roleNames);

    Task<IdentityResult> DeleteAccountAsync(string userId);

    Task<string> GenerateTwoFactorTokenAsync(string email);

    Task<ApplicationUser> GetUserAsync(string userId);

    Task<Result<LoginResponse>> LoginAsync(LoginRequest loginRequest);

    Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);

    Task<Result> RegisterAsync(RegisterRequest registerRequest);

    Task SignOutAsync(string email);

    Task<SignInResult> TwoFactorLoginAsync(string token);

    Task<IdentityResult> SetLockoutEnabledAsync(string email, DateTimeOffset lockoutEnd, bool enabled);
}