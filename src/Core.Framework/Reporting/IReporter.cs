namespace Core.Framework.Reporting;

public interface IReporter
{
    void OnTestStart(string testName);
    void OnLog(string message);
    void OnTestSuccess(string testName);
    void OnTestFailure(string testName, Exception ex);
    void Flush();
}