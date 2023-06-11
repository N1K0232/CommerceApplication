using System;
using System.Text;
using CommerceApi.Security.Abstractions;

namespace CommerceApi.Security
{
    public class StringHasher : IStringHasher
    {
        public string? GetString(string? input)
        {
            if (input is null)
            {
                return null;
            }

            var bytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}