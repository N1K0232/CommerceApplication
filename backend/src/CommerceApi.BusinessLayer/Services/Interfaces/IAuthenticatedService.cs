using System.Security.Claims;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IAuthenticatedService
{
    Task<Result<User>> AddAddressAsync(SaveAddressRequest address);

    Task<Result> AddPhotoAsync(string fileName, Stream stream);

    Task<User> GetAsync();

    Task<IEnumerable<Claim>> GetClaimsAsync();

    Task<ClaimsIdentity> GetIdentityAsync();

    Task<Result<StreamFileContent>> GetPhotoAsync();

    Task<Guid> GetUserIdAsync();

    Task<string> GetUserNameAsync();
}