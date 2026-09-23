using Core.Framework.Config;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Core.Framework.Core.Retries;

public class FlakyRetryDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;

    public FlakyRetryDiscoverer(IMessageSink diagnosticMessageSink)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    public IEnumerable<IXunitTestCase> Discover(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestMethod testMethod,
        IAttributeInfo factAttribute)
    {
        var maxRetryCount = factAttribute.GetNamedArgument<int>("MaxRetryCount");
        if (maxRetryCount <= 0)
        {
            maxRetryCount = FrameworkConfig.Instance.MaxRetryCount;
        }

        yield return new FlakyRetryTestCase(
            _diagnosticMessageSink,
            discoveryOptions.MethodDisplayOrDefault(),
            discoveryOptions.MethodDisplayOptionsOrDefault(),
            testMethod,
            maxRetryCount
        );
    }
}