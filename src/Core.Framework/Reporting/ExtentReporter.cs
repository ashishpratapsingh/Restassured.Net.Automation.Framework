using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace Core.Framework.Reporting;

public class ExtentReporter : IReporter
{
    private static readonly Lazy<ExtentReports> _extent = new(() =>
    {
        // 1. Ensure absolute directory path and automatic directory creation
        string reportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestResults");
        if (!Directory.Exists(reportDir))
        {
            Directory.CreateDirectory(reportDir);
        }

        string reportPath = Path.Combine(reportDir, "ExtentReport.html");
        var htmlReporter = new ExtentSparkReporter(reportPath);

        var reporter = new ExtentReports();
        reporter.AttachReporter(htmlReporter);
        return reporter;
    });

    // 2. Use AsyncLocal<T> instead of ManagedThreadId dictionary for xUnit thread safety
    private static readonly AsyncLocal<ExtentTest?> _currentTest = new();

    public void OnTestStart(string testName)
    {
        var test = _extent.Value.CreateTest(testName);
        _currentTest.Value = test;
    }

    public void OnLog(string message)
    {
        _currentTest.Value?.Info(message);
    }

    public void OnTestSuccess(string testName)
    {
        _currentTest.Value?.Pass("Test Passed Successfully");
    }

    public void OnTestFailure(string testName, Exception ex)
    {
        _currentTest.Value?.Fail(ex);
    }

    public void Flush()
    {
        _extent.Value.Flush();
    }
}