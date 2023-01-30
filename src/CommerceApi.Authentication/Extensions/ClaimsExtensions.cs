using System.Security.Claims;
using System.Security.Principal;
using CommerceApi.Authentication.Common;

namespace CommerceApi.Authentication.Extensions;

public static class ClaimsExtensions
{
    public static string GetApplicationId(this IPrincipal user) => GetClaimValue(user, CustomClaimTypes.ApplicationId);

    public static Guid GetId(this IPrincipal user)
    {
        var value = GetClaimValue(user, ClaimTypes.NameIdentifier);
        if (Guid.TryParse(value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

    public static string GetFirstName(this IPrincipal user) => GetClaimValue(user, ClaimTypes.GivenName);

    public static string GetLastName(this IPrincipal user) => GetClaimValue(user, ClaimTypes.Surname);

    public static DateTime GetDateOfBirth(this IPrincipal user)
    {
        var value = GetClaimValue(user, ClaimTypes.DateOfBirth);
        if (DateTime.TryParse(value, out var dateOfBirth))
        {
            return dateOfBirth;
        }

        return DateTime.MinValue;
    }

    public static int GetAge(this IPrincipal user)
    {
        var value = GetClaimValue(user, CustomClaimTypes.Age);
        if (int.TryParse(value, out var age))
        {
            return age;
        }

        return 0;
    }

    public static string GetUserName(this IPrincipal user) => GetClaimValue(user, ClaimTypes.Name);

    public static string GetEmail(this IPrincipal user) => GetClaimValue(user, ClaimTypes.Email);

    public static string GetPhoneNumber(this IPrincipal user) => GetClaimValue(user, ClaimTypes.MobilePhone);

    public static IEnumerable<string> GetRoles(this IPrincipal user)
    {
        var roles = ((ClaimsPrincipal)user).FindAll(ClaimTypes.Role).Select(r => r.Value);
        return roles;
    }

    public static string GetClaimValue(this IPrincipal user, string claimType)
    {
        if (!user.Identity.IsAuthenticated)
        {
            return null;
        }

        var value = ((ClaimsPrincipal)user).FindFirst(claimType)?.Value;
        return value;
    }
}