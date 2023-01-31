using CommerceApi.Shared.Requests;
using CommerceApi.Shared.Responses;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IUserService : IDisposable
{
    Task<Result<RegisterResponse>> DeleteAccountAsync(Guid userId);

    Task<Result<RegisterResponse>> EnableTwoFactorAuthenticationAsync(TwoFactorRequest request);

    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);

    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);

    Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);

    Task<Result<RegisterResponse>> ValidateEmailAsync(ValidateEmailRequest request);

    Task<Result<RegisterResponse>> ValidatePhoneNumberAsync(ValidatePhoneNumberRequest request);
}