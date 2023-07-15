using CommerceApi.BusinessLayer.RemoteServices.Interfaces;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CommerceApi.BusinessLayer.RemoteServices;

public class KeyService : IKeyService
{
    private readonly IDataProtectionKeyContext _dataProtectionKeyContext;

    public KeyService(IDataProtectionKeyContext dataProtectionKeyContext)
    {
        _dataProtectionKeyContext = dataProtectionKeyContext;
    }

    public async Task<IReadOnlyDictionary<int, DataProtectionKey>> GetListAsync(string friendlyName)
    {
        var query = _dataProtectionKeyContext.DataProtectionKeys.AsQueryable();

        if (!string.IsNullOrWhiteSpace(friendlyName))
        {
            query = query.Where(k => k.FriendlyName.Contains(friendlyName));
        }

        var keys = await query.ToDictionaryAsync(k => k.Id, v => new DataProtectionKey { Id = v.Id, FriendlyName = v.FriendlyName, Xml = v.Xml });
        return keys;
    }
}