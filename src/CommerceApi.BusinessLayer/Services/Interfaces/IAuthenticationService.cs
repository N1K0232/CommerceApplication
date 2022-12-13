using CommerceApi.Shared.Models;

namespace CommerceApi.BusinessLayer.Services.Interfaces;
public interface IAuthenticationService
{
    Task<User> GetAsync(Guid userId);
}