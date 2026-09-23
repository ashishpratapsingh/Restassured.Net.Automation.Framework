using Core.Framework.Core.Context;
using Core.Framework.Reporting;
using RestAssured.Response;

namespace Core.Framework.Core.Execution;

public static class ApiRequestExecutor
{
    public static VerifiableResponse ExecuteSafe(Func<VerifiableResponse> action, string stepDescription)
    {
        TestExecutionContext.Current.Log($"Executing HTTP Call: {stepDescription}");
        ReporterRegistry.DispatchLog($"Executing: {stepDescription}");

        try
        {
            // action() returns VerifiableResponse directly
            var response = action();
            
            TestExecutionContext.Current.Log($"Response Received.");
            ReporterRegistry.DispatchLog($"Response Received successfully.");

            return response;
        }
        catch (Exception ex)
        {
            var formattedException = new InvalidOperationException(
                $"API Framework Execution Error during [{stepDescription}]: {ex.Message}", ex);
            
            TestExecutionContext.Current.Log($"[ERROR] {formattedException.Message}");
            ReporterRegistry.DispatchFailure(TestExecutionContext.Current.TestName, formattedException);
            
            throw formattedException;
        }
    }
}