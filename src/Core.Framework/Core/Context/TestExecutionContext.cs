namespace Core.Framework.Core.Context;

public class TestExecutionContext
{
    private static readonly AsyncLocal<TestExecutionContext?> _currentContext = new();

    public static TestExecutionContext Current =>
        _currentContext.Value ??= new TestExecutionContext();

    public string TestName { get; set; } = string.Empty;
    public string ThreadId => Thread.CurrentThread.ManagedThreadId.ToString();
    public List<string> StepLogs { get; } = new();

    public void Log(string message)
    {
        var formatted = $"[Thread-{ThreadId}] [{DateTime.UtcNow:HH:mm:ss.fff}] {message}";
        StepLogs.Add(formatted);
    }

    public static void Reset()
    {
        _currentContext.Value = null;
    }
}