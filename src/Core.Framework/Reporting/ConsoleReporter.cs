namespace Core.Framework.Reporting;

public class ConsoleReporter : IReporter
{
    public void OnTestStart(string testName) => Console.WriteLine($"[START] {testName}");
    public void OnLog(string message) => Console.WriteLine($"  [LOG] {message}");
    public void OnTestSuccess(string testName) => Console.WriteLine($"[PASS] {testName}");
    public void OnTestFailure(string testName, Exception ex) => Console.WriteLine($"[FAIL] {testName}: {ex.Message}");
    public void Flush() { }
}