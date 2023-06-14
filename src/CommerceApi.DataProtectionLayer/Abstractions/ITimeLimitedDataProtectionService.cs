namespace CommerceApi.DataProtectionLayer.Abstractions;

public interface ITimeLimitedDataProtectionService
{
    string? Protect(string input, TimeSpan lifetime);

    string? Unprotect(string input, TimeSpan lifetime);
}