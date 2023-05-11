using AutoMapper;
using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.Shared.Models.Responses;
using CommerceApi.StorageProviders;
using FluentValidation;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services;

public class UserService : IUserService
{
    private readonly IIdentityService identityService;
    private readonly IStorageProvider storageProvider;
    private readonly IMapper mapper;
    private readonly IValidator<LoginRequest> loginValidator;
    private readonly IValidator<RegisterRequest> registerValidator;
    private readonly IValidator<RefreshTokenRequest> refreshTokenValidator;

    public UserService(IIdentityService identityService,
                       IStorageProvider storageProvider,
                       IMapper mapper,
                       IValidator<LoginRequest> loginValidator,
                       IValidator<RegisterRequest> registerValidator,
                       IValidator<RefreshTokenRequest> refreshTokenValidator)
    {
        this.identityService = identityService;
        this.storageProvider = storageProvider;
        this.mapper = mapper;
        this.loginValidator = loginValidator;
        this.registerValidator = registerValidator;
        this.refreshTokenValidator = refreshTokenValidator;
    }

    public async Task<Result> DeleteAccountAsync(Guid userId)
    {
        var identityResult = await identityService.DeleteAccountAsync(userId.ToString());
        if (identityResult.Succeeded)
        {
            return Result.Ok();
        }

        var validationErrors = new List<ValidationError>();
        foreach (var error in identityResult.Errors)
        {
            validationErrors.Add(new(error.Code, error.Description));
        }

        return Result.Fail(FailureReasons.DatabaseError, validationErrors);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var validationResult = await loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.ClientError, validationErrors);
        }

        var signInResult = await identityService.SignInAsync(request.Email, request.Password);
        if (!signInResult.Succeeded)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid email or password");
        }

        var loginResponse = await identityService.LoginAsync(request.Email);
        return loginResponse;
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var validationResult = await refreshTokenValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.ClientError, validationErrors);
        }

        var principal = await identityService.ValidateAccessTokenAsync(request.AccessToken);

        var loginResponse = await identityService.RefreshTokenAsync(principal, request.RefreshToken);
        return loginResponse;
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        var validationResult = await registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.ClientError, validationErrors);
        }

        var user = mapper.Map<ApplicationUser>(request);
        var identityResult = await identityService.RegisterAsync(user, request.Password, RoleNames.User);

        var registerResponse = new RegisterResponse(identityResult.Succeeded, identityResult.Errors.Select(e => e.Description));
        return registerResponse;
    }

    public async Task<Result> SignOutAsync(string email)
    {
        await identityService.SignOutAsync(email);
        return Result.Ok();
    }

    public async Task<Result> UploadPhotoAsync(Guid userId, string imagePath, Stream imageStream)
    {
        try
        {
            var userExists = await identityService.UserExistsAsync(userId.ToString());
            if (userExists)
            {
                var fullPath = $@"\users\attachments\{userId}_{imagePath}";
                await storageProvider.UploadAsync(fullPath, imageStream);

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