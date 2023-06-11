using Microsoft.AspNetCore.DataProtection;

namespace CommerceApi.DataProtectionLayer;

public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtector _dataProtector;

    public DataProtectionService(IDataProtector dataProtector)
    {
        _dataProtector = dataProtector;
    }

    public Task<string> ProtectAsync(string input)
    {
        var protectedString = _dataProtector.Protect(input);
        return Task.FromResult(protectedString);
    }

    public Task<string> UnprotectAsync(string input)
    {
        var unprotectedString = _dataProtector.Unprotect(input);
        return Task.FromResult(unprotectedString);
    }
}