using System;

namespace CommerceApi.Security.Abstractions
{
    public interface IPasswordHasher : IDisposable
    {
        string? Hash(string? password);

        (bool Verified, bool NeedsUpgrade) Check(string hash, string password);
    }
}