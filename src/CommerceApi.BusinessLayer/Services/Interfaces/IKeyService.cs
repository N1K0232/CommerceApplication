using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IKeyService
{
    Task<DataProtectionKey> GetAsync(string friendlyName);

    Task<IEnumerable<DataProtectionKey>> GetListAsync();
}