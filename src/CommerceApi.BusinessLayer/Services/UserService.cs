using AutoMapper;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Requests;
using CommerceApi.Shared.Responses;
using FluentValidation;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services;

public class UserService : IUserService
{
    private readonly IIdentityService identityService;
    private readonly IMapper mapper;
    private readonly IValidator<LoginRequest> loginValidator;
    private readonly IValidator<RegisterRequest> registerValidator;
    private readonly IValidator<RefreshTokenRequest> refreshTokenValidator;

    public UserService(IIdentityService identityService,
                       IMapper mapper,
                       IValidator<LoginRequest> loginValidator,
                       IValidator<RegisterRequest> registerValidator,
                       IValidator<RefreshTokenRequest> refreshTokenValidator)
    {
        this.identityService = identityService;
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
            return null;
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

        return new LoginResponse();
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

        return new RegisterResponse();
    }

    public async Task<Result> SignOutAsync(string email)
    {
        await identityService.SignOutAsync(email);
        return Result.Ok();
    }
}