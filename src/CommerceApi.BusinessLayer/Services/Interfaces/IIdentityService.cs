using System.Security.Claims;
using CommerceApi.Authentication.Entities;
using CommerceApi.Shared.Responses;
using Microsoft.AspNetCore.Identity;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IIdentityService
{
    Task CreateRoleAsync(string roleName);

    Task CreateRolesAsync(IEnumerable<string> roleNames);

    Task<IdentityResult> DeleteAccountAsync(string userId);

    Task<LoginResponse> LoginAsync(string email);

    Task<LoginResponse> RefreshTokenAsync(ClaimsPrincipal principal, string refreshToken);

    Task<IdentityResult> RegisterAsync(AuthenticationUser user, string password, string role);

    Task<IdentityResult> RegisterAsync(AuthenticationUser user, string password, params string[] roles);

    Task<SignInResult> SignInAsync(string email, string password);

    Task SignOutAsync(string email);

    Task<ClaimsPrincipal> ValidateAccessTokenAsync(string accessToken);
}