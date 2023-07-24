﻿using System.Security.Claims;
using CommerceApi.Authentication.Extensions;
using CommerceApi.SharedServices;
using Microsoft.AspNetCore.Http;

namespace CommerceApi.Authentication.RemoteServices;

internal class UserClaimService : IUserClaimService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserClaimService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext HttpContext
    {
        get
        {
            return _httpContextAccessor.HttpContext;
        }
    }

    public string GetApplicationId() => HttpContext.User.GetApplicationId();

    public Guid GetId() => HttpContext.User.GetId();

    public ClaimsIdentity GetIdentity() => GetIdentityInternal();

    public Guid GetTenantId()
    {
        var tenantIdString = GetClaimValue("TenantId");

        var parsed = Guid.TryParse(tenantIdString, out var tenantId);
        return parsed ? tenantId : Guid.Empty;
    }

    public string GetUserName()
    {
        var identity = GetIdentityInternal();

        var userName = identity.Name ?? HttpContext.User.GetUserName();
        return userName;
    }

    private string GetClaimValue(string claimType)
        => HttpContext.User.GetClaimValueInternal(claimType);

    private ClaimsIdentity GetIdentityInternal()
    {
        var identity = HttpContext.User.Identity;
        return identity as ClaimsIdentity;
    }
}