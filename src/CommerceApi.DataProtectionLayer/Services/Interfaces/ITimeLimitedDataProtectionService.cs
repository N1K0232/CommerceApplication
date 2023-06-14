namespace CommerceApi.DataProtectionLayer.Services.Interfaces;

public interface ITimeLimitedDataProtectionService
{
    string? Protect(string input, TimeSpan lifetime);

    string? Unprotect(string input, TimeSpan lifetime);
}