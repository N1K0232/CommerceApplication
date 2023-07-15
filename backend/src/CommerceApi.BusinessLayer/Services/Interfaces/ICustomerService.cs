using CommerceApi.Shared.Models.Requests;
using CommerceApi.Shared.Models.Responses;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface ICustomerService
{
    Task<Result> DeleteAsync(Guid customerId);

    Task<LoginResponse> LoginAsync(LoginRequest loginRequest);

    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);

    Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest);
}