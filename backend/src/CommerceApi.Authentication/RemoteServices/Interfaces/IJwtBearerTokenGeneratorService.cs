using System.Security.Claims;
using CommerceApi.Authentication.Settings;

namespace CommerceApi.Authentication.RemoteServices.Interfaces;

public interface IJwtBearerTokenGeneratorService
{
    JwtSettings JwtSettings { get; }

    string GenerateAccessToken(IEnumerable<Claim> claims);

    string GenerateRefreshToken();

    ClaimsPrincipal ValidateAccessToken(string accessToken);
}