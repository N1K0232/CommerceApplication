using System.Security.Claims;
using AutoMapper;
using CommerceApi.Authentication.Managers;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models;
using CommerceApi.SharedServices;

namespace CommerceApi.BusinessLayer.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationUserManager _userManager;
    private readonly IUserClaimService _claimService;
    private readonly IMapper _mapper;

    public AuthenticationService(ApplicationUserManager userManager, IUserClaimService claimService, IMapper mapper)
    {
        _userManager = userManager;
        _claimService = claimService;
        _mapper = mapper;
    }


    public async Task<User> GetAsync()
    {
        var userId = await GetUserIdCoreAsync();
        var dbUser = await _userManager.FindByIdAsync(userId.ToString());

        var user = _mapper.Map<User>(dbUser);
        return user;
    }

    public Task<Guid> GetUserIdAsync() => GetUserIdCoreAsync();

    public Task<string> GetUserNameAsync()
    {
        var userName = _claimService.GetUserName();
        return Task.FromResult(userName);
    }

    public Task<ClaimsIdentity> GetIdentityAsync() => GetIdentityCoreAsync();

    public async Task<IEnumerable<Claim>> GetClaimsAsync()
    {
        var identity = await GetIdentityCoreAsync();
        return identity.Claims;
    }

    private Task<Guid> GetUserIdCoreAsync()
    {
        var userId = _claimService.GetId();
        return Task.FromResult(userId);
    }

    private Task<ClaimsIdentity> GetIdentityCoreAsync()
    {
        var identity = _claimService.GetIdentity();
        return Task.FromResult(identity);
    }
}