using System.Text.Json.Serialization;

namespace CommerceApi.Shared.Models.Responses;

public class LoginResponse
{
    [JsonConstructor]
    public LoginResponse()
    {
    }

    public LoginResponse(string? accessToken, string? refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }


    public string? AccessToken { get; init; }

    public string? RefreshToken { get; init; }
}