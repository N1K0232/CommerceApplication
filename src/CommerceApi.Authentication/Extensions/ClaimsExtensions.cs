using System.Security.Claims;
using System.Security.Principal;
using CommerceApi.Authentication.Common;

namespace CommerceApi.Authentication.Extensions;

public static class ClaimsExtensions
{
    public static string GetApplicationId(this IPrincipal user)
        => GetClaimValueInternal(user, CustomClaimTypes.ApplicationId);

    public static Guid GetId(this IPrincipal user)
    {
        var value = GetClaimValueInternal(user, ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }

    public static string GetFirstName(this IPrincipal user)
        => GetClaimValueInternal(user, ClaimTypes.GivenName);

    public static string GetLastName(this IPrincipal user)
        => GetClaimValueInternal(user, ClaimTypes.Surname);

    public static DateTime GetDateOfBirth(this IPrincipal user)
    {
        var value = GetClaimValueInternal(user, ClaimTypes.DateOfBirth);
        return DateTime.TryParse(value, out var dateOfBirth) ? dateOfBirth : DateTime.MinValue;
    }

    public static int GetAge(this IPrincipal user)
    {
        var value = GetClaimValueInternal(user, CustomClaimTypes.Age);
        return int.TryParse(value, out var age) ? age : 0;
    }

    public static string GetUserName(this IPrincipal user)
        => GetClaimValueInternal(user, ClaimTypes.Name);

    public static string GetEmail(this IPrincipal user)
        => GetClaimValueInternal(user, ClaimTypes.Email);

    public static string GetPhoneNumber(this IPrincipal user)
        => GetClaimValueInternal(user, ClaimTypes.MobilePhone);

    public static IEnumerable<string> GetRoles(this IPrincipal user)
    {
        var roleClaims = (user as ClaimsPrincipal).FindAll(ClaimTypes.Role);
        return roleClaims.Select(c => c.Value);
    }

    public static string GetClaimValue(this IPrincipal user, string claimType)
        => GetClaimValueInternal(user, claimType);

    internal static string GetClaimValueInternal(this IPrincipal user, string claimType)
    {
        if (!user.Identity.IsAuthenticated)
        {
            return null;
        }

        var claim = (user as ClaimsPrincipal).FindFirst(claimType);
        return claim?.Value ?? string.Empty;
    }
}