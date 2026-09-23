using Core.Framework.Config;

namespace Core.Framework.Reporting;
public static class ReporterRegistry
{
    private static readonly List<IReporter> _reporters = new();

    static ReporterRegistry()
    {
        var options = FrameworkConfig.Instance.EnabledReporters;
        if (options.Contains("Extent")) _reporters.Add(new ExtentReporter());
        if (options.Contains("Console")) _reporters.Add(new ConsoleReporter());
    }

    public static void DispatchStart(string testName) => _reporters.ForEach(r => r.OnTestStart(testName));
    public static void DispatchLog(string message) => _reporters.ForEach(r => r.OnLog(message));
    public static void DispatchSuccess(string testName) => _reporters.ForEach(r => r.OnTestSuccess(testName));
    public static void DispatchFailure(string testName, Exception ex) => _reporters.ForEach(r => r.OnTestFailure(testName, ex));
    public static void DispatchFlush() => _reporters.ForEach(r => r.Flush());
}