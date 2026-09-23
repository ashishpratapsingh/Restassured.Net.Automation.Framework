using Core.Framework.Config;
using Xunit;
using Xunit.Sdk;

namespace Core.Framework.Core.Retries;

/// <summary>
/// Custom xUnit Fact attribute that retries failed tests up to MaxRetryCount configured in appsettings.json.
/// </summary>
[XunitTestCaseDiscoverer("Core.Framework.Core.Retries.FlakyRetryDiscoverer", "Core.Framework")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FlakyRetryFactAttribute : FactAttribute
{
    public int MaxRetryCount { get; }

    public FlakyRetryFactAttribute()
    {
        MaxRetryCount = FrameworkConfig.Instance.MaxRetryCount;
    }

    public FlakyRetryFactAttribute(int maxRetryCount)
    {
        MaxRetryCount = maxRetryCount;
    }
}