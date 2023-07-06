using AutoMapper;
using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.Shared.Models.Responses;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services;

public class CustomerService : ICustomerService
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public CustomerService(IIdentityService identityService, IMapper mapper)
    {
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "invalid id");
        }

        var deleteResult = await _identityService.DeleteAccountAsync(customerId.ToString());
        return deleteResult.Succeeded ? Result.Ok() : Result.Fail(FailureReasons.DatabaseError, "couldn't delete account");
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        var signInAsync = await _identityService.SignInAsync(loginRequest.Email, loginRequest.Password);
        if (!signInAsync.Succeeded)
        {
            return null;
        }

        var loginResponse = await _identityService.LoginAsync(loginRequest.Email);
        return loginResponse;
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
    {
        var principal = await _identityService.ValidateAccessTokenAsync(refreshTokenRequest.AccessToken);
        if (principal == null)
        {
            return null;
        }

        var loginResponse = await _identityService.RefreshTokenAsync(principal, refreshTokenRequest.RefreshToken);
        return loginResponse;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest)
    {
        var user = _mapper.Map<ApplicationUser>(registerRequest);
        var registerResult = await _identityService.RegisterAsync(user, registerRequest.Password, RoleNames.Customer, RoleNames.User);

        var succeeded = registerResult.Succeeded;
        var errors = registerResult.Errors.Select(e => e.Description);

        var registerResponse = new RegisterResponse { Succeeded = succeeded, Errors = errors };
        return registerResponse;
    }
}