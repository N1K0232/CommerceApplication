﻿using CommerceApi.Shared.Requests;
using CommerceApi.Shared.Responses;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IUserService
{
    Task<RegisterResponse> DeleteAccountAsync(Guid userId);

    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<RegisterResponse> RegisterAsync(RegisterRequest request);

    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
}