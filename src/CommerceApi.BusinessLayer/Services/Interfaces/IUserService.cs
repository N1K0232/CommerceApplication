using CommerceApi.Shared.Requests;
using CommerceApi.Shared.Responses;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IUserService : IDisposable
{
    Task<RegisterResponse> DeleteAccountAsync(Guid userId);

    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);

    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);

    Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
}