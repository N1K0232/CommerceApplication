namespace CommerceApi.Shared.Responses;

public class LoginResponse
{
    public LoginResponse(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public string AccessToken { get; }

    public string RefreshToken { get; }
}