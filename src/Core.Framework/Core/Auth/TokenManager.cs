using System.Text.Json;
using Core.Framework.Config;
using RestAssured.Request.Builders;
using static RestAssured.Dsl;

namespace Core.Framework.Core.Auth;

public static class TokenManager
{
    private static CachedToken? _cachedToken;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Thread-safe entry point to fetch a valid Bearer token.
    /// Caches existing tokens and auto-refreshes when expired.
    /// </summary>
    public static string GetValidToken()
    {
        // 1. Lock-free check for fast parallel execution
        if (_cachedToken != null && !_cachedToken.IsExpired)
        {
            return _cachedToken.AccessToken;
        }

        // 2. Thread synchronization point for token fetch
        _semaphore.Wait();
        try
        {
            // Double-checked locking pattern
            if (_cachedToken != null && !_cachedToken.IsExpired)
            {
                return _cachedToken.AccessToken;
            }

            _cachedToken = RequestNewToken();
            return _cachedToken.AccessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static CachedToken RequestNewToken()
    {
        var cfg = FrameworkConfig.Instance;

        var httpResponseMessage = Given()
            .Spec(new RequestSpecBuilder().WithBaseUri(cfg.TokenUrl).Build())
            .ContentType("application/x-www-form-urlencoded") // REPLACED .Header(...) WITH .ContentType(...)
            .FormData(new Dictionary<string, string>
            {
                { "grant_type", cfg.GrantType },
                { "client_id", cfg.ClientId },
                { "client_secret", cfg.ClientSecret },
                { "scope", cfg.Scope }
            })
            .When()
            .Post("")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();

        var rawJson = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (string.IsNullOrEmpty(rawJson))
        {
            throw new InvalidOperationException("Received empty response body from Auth Server.");
        }

        var response = JsonSerializer.Deserialize<TokenResponse>(rawJson);

        if (response == null || string.IsNullOrEmpty(response.AccessToken))
        {
            throw new InvalidOperationException("Failed to acquire OAuth2 access token from Auth Server.");
        }

        return new CachedToken(response.AccessToken, response.ExpiresIn);
    }
}