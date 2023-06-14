namespace CommerceApi.DataProtectionLayer.Services.Interfaces;

public interface IDataProtectionService
{
    string? Protect(string input);

    string? Unprotect(string input);
}