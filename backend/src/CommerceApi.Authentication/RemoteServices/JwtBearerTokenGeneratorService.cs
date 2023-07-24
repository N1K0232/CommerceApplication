using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CommerceApi.Authentication.RemoteServices.Interfaces;
using CommerceApi.Authentication.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CommerceApi.Authentication.RemoteServices;

internal class JwtBearerTokenGeneratorService : IJwtBearerTokenGeneratorService
{
    public JwtBearerTokenGeneratorService(IOptions<JwtSettings> jwtSettingsOptions)
    {
        JwtSettings = jwtSettingsOptions.Value;
    }

    public JwtSettings JwtSettings { get; }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var securityKeyBytes = Encoding.UTF8.GetBytes(JwtSettings.SecurityKey);
        var securityKey = new SymmetricSecurityKey(securityKeyBytes);

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(JwtSettings.Issuer, JwtSettings.Audience, claims,
            DateTime.UtcNow, DateTime.UtcNow.AddMinutes(JwtSettings.AccessTokenExpirationMinutes), credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return accessToken;
    }

    public string GenerateRefreshToken()
    {
        using var generator = RandomNumberGenerator.Create();
        var randomNumber = new byte[256];

        generator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal ValidateAccessToken(string accessToken)
    {
        var securityKeyBytes = Encoding.UTF8.GetBytes(JwtSettings.SecurityKey);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtSettings.Audience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(securityKeyBytes),
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var user = tokenHandler.ValidateToken(accessToken, parameters, out var securityToken);
            if (securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                return user;
            }
        }
        catch
        {
        }

        return null;
    }
}