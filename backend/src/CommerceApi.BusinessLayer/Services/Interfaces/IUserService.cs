using CommerceApi.Shared.Models.Requests;
using CommerceApi.Shared.Models.Responses;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IUserService
{
    Task<Result> DeleteAccountAsync(Guid userId);

    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);

    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);

    Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);

    Task<Result> SignOutAsync(string email);

    Task<Result> UploadPhotoAsync(Guid userId, string imagePath, Stream imageStream);
}