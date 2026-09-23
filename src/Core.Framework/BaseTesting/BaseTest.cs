using Core.Framework.Core.Context;
using Core.Framework.Reporting;
using Xunit;

// Configures xUnit to run test classes in parallel with up to 4 worker threads
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass, MaxParallelThreads = 4)]

namespace Core.Framework.BaseTesting;

public abstract class BaseTest : IDisposable
{
    protected BaseTest()
    {
        // Setup: Executes before each test
        var testName = GetType().Name;
        TestExecutionContext.Current.TestName = testName;
        ReporterRegistry.DispatchStart(testName);
    }

    /// <summary>
    /// Teardown: Executes after each test completes.
    /// </summary>
    public void Dispose()
    {
        var testName = TestExecutionContext.Current.TestName;

        // xUnit handles test exceptions outside IDisposable.Dispose().
        ReporterRegistry.DispatchSuccess(testName);

        // Flush report state and reset thread context
        ReporterRegistry.DispatchFlush();
        TestExecutionContext.Reset();

        GC.SuppressFinalize(this);
    }
}