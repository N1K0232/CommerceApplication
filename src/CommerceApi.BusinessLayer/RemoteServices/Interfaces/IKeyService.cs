using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace CommerceApi.BusinessLayer.RemoteServices.Interfaces;

public interface IKeyService
{
    Task<IReadOnlyDictionary<int, DataProtectionKey>> GetListAsync(string friendlyName);
}