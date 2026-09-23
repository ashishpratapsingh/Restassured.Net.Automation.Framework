using System.Text.Json.Serialization;

namespace Core.Framework.Core.Auth;
public record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);

public class CachedToken
{
    public string AccessToken { get; }
    public DateTime ExpiryTimeUtc { get; }

    public CachedToken(string accessToken, int expiresInSeconds)
    {
        AccessToken = accessToken;
        // Subtract 30 seconds buffer to prevent edge-case expiration during request transit
        ExpiryTimeUtc = DateTime.UtcNow.AddSeconds(expiresInSeconds - 30);
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiryTimeUtc;
}