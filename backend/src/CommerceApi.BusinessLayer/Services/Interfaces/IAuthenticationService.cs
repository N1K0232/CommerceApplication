using System.Security.Claims;
using CommerceApi.Shared.Models;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IAuthenticationService
{
    Task<User> GetAsync();

    Task<Guid> GetUserIdAsync();

    Task<string> GetUserNameAsync();

    Task<ClaimsIdentity> GetIdentityAsync();

    Task<IEnumerable<Claim>> GetClaimsAsync();
}