using System.Security.Claims;
using AutoMapper;
using CommerceApi.Authentication.Entities;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models;
using CommerceApi.SharedServices;
using Microsoft.AspNetCore.Identity;

namespace CommerceApi.BusinessLayer.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IUserClaimService claimService;
    private readonly IMapper mapper;

    public AuthenticationService(UserManager<ApplicationUser> userManager, IUserClaimService claimService, IMapper mapper)
    {
        this.userManager = userManager;
        this.claimService = claimService;
        this.mapper = mapper;
    }


    public async Task<User> GetAsync()
    {
        var userId = await GetUserIdCoreAsync();
        var dbUser = await userManager.FindByIdAsync(userId.ToString());

        var user = mapper.Map<User>(dbUser);
        return user;
    }

    public Task<Guid> GetUserIdAsync() => GetUserIdCoreAsync();

    public Task<string> GetUserNameAsync()
    {
        var userName = claimService.GetUserName();
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
        var userId = claimService.GetId();
        return Task.FromResult(userId);
    }

    private Task<ClaimsIdentity> GetIdentityCoreAsync()
    {
        var identity = claimService.GetIdentity();
        return Task.FromResult(identity);
    }
}