using CommerceApi.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CommerceApi.BusinessLayer.Services;

public class KeyService : IKeyService
{
    private readonly IDataProtectionKeyContext _dataProtectionKeyContext;

    public KeyService(IDataProtectionKeyContext dataProtectionKeyContext)
    {
        _dataProtectionKeyContext = dataProtectionKeyContext;
    }

    public async Task<DataProtectionKey> GetAsync(string friendlyName)
    {
        var query = _dataProtectionKeyContext.DataProtectionKeys.AsQueryable();

        var key = await query.FirstOrDefaultAsync(k => k.FriendlyName == friendlyName);
        return key;
    }

    public async Task<IEnumerable<DataProtectionKey>> GetListAsync()
    {
        var query = _dataProtectionKeyContext.DataProtectionKeys.AsQueryable();

        var keys = await query.OrderBy(k => k.Id).ToListAsync();
        return keys;
    }
}