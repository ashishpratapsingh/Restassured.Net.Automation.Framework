using System.ComponentModel;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Core.Framework.Core.Retries;

public class FlakyRetryTestCase : XunitTestCase
{
    private int _maxRetryCount;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Called by xUnit deserializer", true)]
    public FlakyRetryTestCase() { }

    public FlakyRetryTestCase(
        IMessageSink diagnosticMessageSink,
        TestMethodDisplay defaultMethodDisplay,
        TestMethodDisplayOptions defaultMethodDisplayOptions,
        ITestMethod testMethod,
        int maxRetryCount)
        : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
    {
        _maxRetryCount = maxRetryCount;
    }

    public override async Task<RunSummary> RunAsync(
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        var delayedMessageBus = new DelayedMessageBus(messageBus);
        RunSummary summary = new RunSummary();

        for (int attempt = 1; attempt <= _maxRetryCount; attempt++)
        {
            summary = await base.RunAsync(
                diagnosticMessageSink,
                delayedMessageBus,
                constructorArguments,
                aggregator,
                cancellationTokenSource
            );

            // If the test passed, flush queued messages and complete
            if (summary.Failed == 0)
            {
                delayedMessageBus.Dispose();
                return summary;
            }

            // On the last failed attempt, flush failure messages to reporting
            if (attempt == _maxRetryCount)
            {
                delayedMessageBus.Dispose();
                return summary;
            }

            // Reset buffered messages for the next retry attempt
            delayedMessageBus.Reset();
        }

        return summary;
    }

    public override void Serialize(IXunitSerializationInfo info)
    {
        base.Serialize(info);
        info.AddValue("MaxRetryCount", _maxRetryCount);
    }

    public override void Deserialize(IXunitSerializationInfo info)
    {
        base.Deserialize(info);
        _maxRetryCount = info.GetValue<int>("MaxRetryCount");
    }
}