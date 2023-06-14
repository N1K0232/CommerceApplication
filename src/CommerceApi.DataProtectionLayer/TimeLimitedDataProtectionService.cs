using CommerceApi.DataProtectionLayer.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace CommerceApi.DataProtectionLayer;

public class TimeLimitedDataProtectionService : ITimeLimitedDataProtectionService
{
    private readonly ITimeLimitedDataProtector _dataProtector;

    public TimeLimitedDataProtectionService(ITimeLimitedDataProtector dataProtector)
    {
        _dataProtector = dataProtector;
    }

    public string? Protect(string input, TimeSpan lifetime)
    {
        if (input is null)
        {
            return null;
        }

        var protectedString = _dataProtector.Protect(input, lifetime);
        return protectedString;
    }

    public string? Unprotect(string input, TimeSpan lifetime)
    {
        if (input is null)
        {
            return null;
        }

        var unprotectedString = _dataProtector.Protect(input, lifetime);
        return unprotectedString;
    }
}