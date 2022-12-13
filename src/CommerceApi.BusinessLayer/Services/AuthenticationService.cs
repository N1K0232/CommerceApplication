using AutoMapper;
using CommerceApi.Authentication.Entities;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace CommerceApi.BusinessLayer.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<AuthenticationUser> userManager;
    private readonly IMapper mapper;

    public AuthenticationService(UserManager<AuthenticationUser> userManager, IMapper mapper)
    {
        this.userManager = userManager;
        this.mapper = mapper;
    }


    public async Task<User> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        var dbUser = await userManager.FindByIdAsync(userId.ToString());

        var user = mapper.Map<User>(dbUser);
        return user;
    }
}