using Core.Framework.Config;
using Core.Framework.Core.Auth;
using RestAssured.Request.Builders;

namespace Core.Framework.BaseApiObject;

public abstract class AuthenticatedApiObject : BaseApiObject
{
    protected RequestSpecification AuthenticatedRequestSpec
    {
        get
        {
            var bearerToken = TokenManager.GetValidToken();

            return new RequestSpecBuilder()
                .WithBaseUri(FrameworkConfig.Instance.BaseUrl)
                .WithOAuth2(bearerToken)
                .WithContentType("application/json") // Use .WithContentType(), NOT .WithHeader("Content-Type", ...)
                .WithTimeout(TimeSpan.FromSeconds(FrameworkConfig.Instance.TimeoutSeconds))
                .Build();
        }
    }
}