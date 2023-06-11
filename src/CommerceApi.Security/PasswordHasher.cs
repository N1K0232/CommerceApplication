using System;
using System.Linq;
using System.Security.Cryptography;
using CommerceApi.Security.Abstractions;

namespace CommerceApi.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; //128 bit
        private const int KeySize = 32; //256 bit
        private const int Iterations = 10000;

        private Rfc2898DeriveBytes _algorithm = null!;
        private bool _disposed = false;

        public PasswordHasher()
        {
        }

        public string? Hash(string? password)
        {
            if (password is null)
            {
                return null;
            }

            _algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA512);
            var bytes = _algorithm.GetBytes(KeySize);

            var key = Convert.ToBase64String(bytes);
            var salt = Convert.ToBase64String(_algorithm.Salt);
            return $"{Iterations}.{salt}.{key}";
        }

        public (bool Verified, bool NeedsUpgrade) Check(string hash, string password)
        {
            var parts = hash.Split('.', 3);
            if (parts.Length != 3)
            {
                throw new FormatException("Invalid string input");
            }

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var needsUpgrade = iterations != Iterations;
            _algorithm = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512);

            var keyToCheck = _algorithm.GetBytes(KeySize);
            var verified = keyToCheck.SequenceEqual(key);
            return (verified, needsUpgrade);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_algorithm != null)
                {
                    _algorithm.Dispose();
                    _algorithm = null!;
                }

                _disposed = true;
            }
        }
    }
}