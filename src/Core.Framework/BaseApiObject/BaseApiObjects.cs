using Core.Framework.Config;
using RestAssured.Request.Builders;

namespace Core.Framework.BaseApiObject;

public abstract class BaseApiObject
{
    protected RequestSpecification RequestSpec { get; }

    protected BaseApiObject()
    {
        RequestSpec = new RequestSpecBuilder()
            .WithBaseUri(FrameworkConfig.Instance.BaseUrl)
            .WithContentType("application/json") // Use .WithContentType(), NOT .WithHeader("Content-Type", ...)
            .WithTimeout(TimeSpan.FromSeconds(FrameworkConfig.Instance.TimeoutSeconds))
            .Build();
    }
}