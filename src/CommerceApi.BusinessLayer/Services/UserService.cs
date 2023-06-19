﻿using AutoMapper;
using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.BusinessLayer.Extensions;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.Shared.Models.Responses;
using CommerceApi.StorageProviders.Abstractions;
using FluentValidation;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services;

public class UserService : IUserService
{
    private readonly IIdentityService _identityService;
    private readonly IStorageProvider _storageProvider;
    private readonly IMapper _mapper;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenValidator;

    public UserService(IIdentityService identityService,
                       IStorageProvider storageProvider,
                       IMapper mapper,
                       IValidator<LoginRequest> loginValidator,
                       IValidator<RegisterRequest> registerValidator,
                       IValidator<RefreshTokenRequest> refreshTokenValidator)
    {
        _identityService = identityService;
        _storageProvider = storageProvider;
        _mapper = mapper;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _refreshTokenValidator = refreshTokenValidator;
    }

    public async Task<Result> DeleteAccountAsync(Guid userId)
    {
        var identityResult = await _identityService.DeleteAccountAsync(userId.ToString());
        if (identityResult.Succeeded)
        {
            return Result.Ok();
        }

        var validationErrors = identityResult.ToValidationErrors();
        return Result.Fail(FailureReasons.DatabaseError, validationErrors);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.ToValidationErrors();
            return Result.Fail(FailureReasons.ClientError, validationErrors);
        }

        var signInResult = await _identityService.SignInAsync(request.Email, request.Password);
        if (signInResult.Succeeded)
        {
            return await _identityService.LoginAsync(request.Email);
        }

        if (signInResult.IsLockedOut)
        {
            return Result.Fail(FailureReasons.GenericError, "you're locked out!");
        }

        if (signInResult.RequiresTwoFactor)
        {
            var twoFactorToken = await _identityService.GenerateTwoFactorTokenAsync(request.Email);
            signInResult = await _identityService.TwoFactorLoginAsync(twoFactorToken);
            return signInResult.Succeeded ? await _identityService.LoginAsync(request.Email) : null;
        }

        return Result.Fail(FailureReasons.ClientError, "Invalid email or password");
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.ToValidationErrors();
            return Result.Fail(FailureReasons.ClientError, validationErrors);
        }

        var principal = await _identityService.ValidateAccessTokenAsync(request.AccessToken);
        return await _identityService.RefreshTokenAsync(principal, request.RefreshToken);
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.ToValidationErrors();
            return Result.Fail(FailureReasons.ClientError, validationErrors);
        }

        var user = _mapper.Map<ApplicationUser>(request);
        var address = _mapper.Map<Address>(request);

        var identityResult = await _identityService.RegisterUserAsync(user, address, request.Password, RoleNames.User);
        return new RegisterResponse(identityResult.Succeeded, identityResult.Errors.Select(e => e.Description));
    }

    public async Task<Result> SignOutAsync(string email)
    {
        await _identityService.SignOutAsync(email);
        return Result.Ok();
    }

    public async Task<Result> UploadPhotoAsync(Guid userId, string imagePath, Stream imageStream)
    {
        try
        {
            var user = await _identityService.GetUserAsync(userId.ToString());
            if (user != null)
            {
                var fullPath = $@"\users\attachments\{userId}_{imagePath}";
                await _storageProvider.SaveAsync(fullPath, imageStream);

                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ItemNotFound, "No user exists");
        }
        catch (SystemException ex)
        {
            return Result.Fail(FailureReasons.GenericError, ex);
        }
    }
}