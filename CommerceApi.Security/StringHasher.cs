using System.Text;

namespace CommerceApi.Security;

public static class StringHasher
{
    public static string GetString(string input)
    {
        var bytes = Convert.FromBase64String(input);
        var originalString = Encoding.UTF8.GetString(bytes);
        return originalString;
    }
}