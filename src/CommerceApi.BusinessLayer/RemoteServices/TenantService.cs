using CommerceApi.Authentication;
using CommerceApi.BusinessLayer.Models;
using CommerceApi.BusinessLayer.RemoteServices.Interfaces;
using CommerceApi.SharedServices;
using Microsoft.Extensions.Caching.Memory;

namespace CommerceApi.BusinessLayer.RemoteServices;

public class TenantService : ITenantService
{
    private readonly AuthenticationDbContext _authenticationDbContext;
    private readonly IUserClaimService _claimService;
    private readonly IMemoryCache _cache;

    public TenantService(AuthenticationDbContext authenticationDbContext, IUserClaimService claimService, IMemoryCache cache)
    {
        _authenticationDbContext = authenticationDbContext;
        _claimService = claimService;
        _cache = cache;
    }

    public Tenant Get()
    {
        var tenants = _cache.GetOrCreate("tenants", entry =>
        {
            var tenants = _authenticationDbContext.Tenants
                .ToDictionary(k => k.Id, v => new Tenant(v.Id, v.Name, v.ConnectionString, v.StorageConnectionString, v.ContainerName));

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return tenants;
        });

        var tenantId = _claimService.GetTenantId();
        if (tenants.TryGetValue(tenantId, out var tenant))
        {
            return tenant;
        }

        return null;
    }
}