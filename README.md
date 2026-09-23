# Enterprise REST API Automation Framework (.NET & RestAssured.Net)

A thread-safe, enterprise-grade REST API automation framework built with **C# .NET**, **xUnit**, **RestAssured.Net**, and **ExtentReports**. Designed with the **API Object Model (AOM)** pattern, atomic OAuth2 token lifecycle management, custom flaky test retry mechanisms, and `AsyncLocal`-isolated reporting for parallel test execution.

---

## 🛠️ Tech Stack & Key Libraries

* **Language & Runtime:** C# .NET 8.0 / .NET 10.0
* **Test Runner:** xUnit
* **HTTP Client & DSL:** RestAssured.Net
* **Reporting:** AventStack ExtentReports (Spark)
* **Configuration:** `Microsoft.Extensions.Configuration` (JSON-based)
* **Serialization:** `System.Text.Json`
* **JSON Schema Validation:** `NJsonSchema` / RestAssured.Net Schema Validator

---

## 🏗️ Architecture & Project Structure

The framework follows a modular, layer-separated structure isolating core framework mechanisms from test scripts and API domain models.

```text
ApiAutomationFramework/
├── Core.Framework/
│   ├── BaseApiObject/
│   │   ├── BaseApiObject.cs             # Unauthenticated base spec builder
│   │   └── AuthenticatedApiObject.cs    # Bearer token injected request spec
│   ├── Config/
│   │   └── FrameworkConfig.cs           # Strongly-typed appsettings mapping
│   ├── Core/
│   │   ├── Auth/
│   │   │   └── TokenManager.cs          # Thread-safe OAuth2 token caching with SemaphoreSlim
│   │   ├── Execution/
│   │   │   └── ApiRequestExecutor.cs    # Safe wrapper with exception mapping & logging
│   │   └── Retries/
│   │       ├── FlakyRetryFactAttribute.cs # Custom xUnit retry attribute
│   │       ├── FlakyRetryDiscoverer.cs    # Test case discoverer for retries
│   │       └── FlakyRetryTestCase.cs      # Custom xUnit test case runner
│   ├── Reporting/
│   │   ├── IReporter.cs                 # Reporter interface abstraction
│   │   └── ExtentReporter.cs            # Thread-safe ExtentReports engine using AsyncLocal
│   └── Schemas/
│       └── user_schema.json             # JSON schema validation files
├── Core.Framework.XunitTest/
│   ├── ApiObjects/
│   │   └── Users/
│   │       ├── SecureUsersApi.cs        # Authenticated API endpoints wrapper
│   │       └── Models/
│   │           └── CreateUserRequest.cs # DTOs with [JsonPropertyName] mapping
│   ├── Tests/
│   │   ├── BaseTest.cs                  # xUnit lifecycle, setup/teardown & parallel execution
│   │   └── UserManagementTests.cs       # Business test cases
│   └── appsettings.json                 # Environment configuration
└── ApiAutomationFramework.sln           # Solution configuration
```

---

## ⚡ Key Architectural Features

### 1. Thread-Safe ExtentReports (`AsyncLocal<ExtentTest>`)
Standard thread-ID tracking (`Thread.CurrentThread.ManagedThreadId`) breaks under xUnit due to asynchronous thread-pool context switching. The `ExtentReporter` uses **`AsyncLocal<ExtentTest?>`** to flow test context seamlessly across async continuations and parallel test runners.

### 2. OAuth2 Token Caching (`SemaphoreSlim`)
Token fetching uses a double-check lock pattern with **`SemaphoreSlim`** to guarantee atomic token requests across parallel threads. Tokens are automatically refreshed prior to expiration.

### 3. Custom Flaky Test Retries (`[FlakyRetryFact]`)
Native xUnit lacks per-test retry logic. The framework includes a custom xUnit test discoverer (`FlakyRetryDiscoverer`) and runner (`FlakyRetryTestCase`) that automatically retries failed assertions up to `MaxRetryCount`.

### 4. Correct Header Handling in RestAssured.Net
In .NET `HttpClient`, `Content-Type` is strictly a content header (`HttpContent.Headers`), not a request header. Requests explicitly configure body format via `.ContentType("application/json")` or `.WithContentType(...)` on builders to prevent runtime header misuse exceptions.

---

## ⚙️ Configuration (`appsettings.json`)

Set your target environment settings in `appsettings.json`:

```json
{
  "FrameworkConfig": {
    "BaseUrl": "https://reqres.in",
    "TokenUrl": "https://identity.yourdomain.com/connect/token",
    "GrantType": "client_credentials",
    "ClientId": "automation_client_id",
    "ClientSecret": "automation_client_secret",
    "Scope": "api.read api.write",
    "TimeoutSeconds": 30,
    "MaxRetryCount": 2
  }
}
```

---

## 🚀 Getting Started & Local Execution

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or higher
* IDE: Visual Studio 2022 / VS Code / JetBrains Rider

### Setup & Run
1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-org/ApiAutomationFramework.git
   cd ApiAutomationFramework
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the solution:**
   ```bash
   dotnet build --no-restore
   ```

4. **Execute tests:**
   ```bash
   dotnet test --logger "console;verbosity=detailed"
   ```

---

## 📝 Writing Tests & API Objects

### 1. Create Request Models (DTO)
Use `[JsonPropertyName]` to ensure correct JSON casing during serialization:

```csharp
using System.Text.Json.Serialization;

namespace Core.Framework.XunitTest.Users.Models;

public record CreateUserRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("job")] string Job
);
```

### 2. Define an API Object
Inherit from `AuthenticatedApiObject` (for OAuth2-protected routes) or `BaseApiObject` (for public routes):

```csharp
using Core.Framework.BaseApiObject;
using Core.Framework.Core.Execution;
using Core.Framework.XunitTest.Users.Models;
using RestAssured.Response;
using static RestAssured.Dsl;

namespace Core.Framework.XunitTest.Users;

public class SecureUsersApi : AuthenticatedApiObject
{
    private const string UsersEndpoint = "/api/users";

    public VerifiableResponse CreateSecureUser(CreateUserRequest requestPayload)
    {
        return ApiRequestExecutor.ExecuteSafe(
            () => Given()
                .Spec(AuthenticatedRequestSpec)
                .ContentType("application/json")
                .Body(requestPayload)
                .When()
                .Post(UsersEndpoint),
            $"POST {UsersEndpoint} [Authenticated]"
        );
    }
}
```

### 3. Write an xUnit Test Case
Inherit from `BaseTest` for setup/teardown execution and apply `[FlakyRetryFact]`:

```csharp
using System.Net;
using Core.Framework.Core.Retries;
using Core.Framework.XunitTest.Users;
using Core.Framework.XunitTest.Users.Models;
using Xunit;

namespace Core.Framework.XunitTest;

public class UserManagementTests : BaseTest
{
    private readonly SecureUsersApi _usersApi;

    public UserManagementTests()
    {
        _usersApi = new SecureUsersApi();
    }

    [FlakyRetryFact]
    public void CreateUser_ShouldMatchDefinedJsonSchema()
    {
        // Arrange
        var requestPayload = new CreateUserRequest("Jane Doe", "Principal Architect");

        // Act & Assert
        _usersApi.CreateSecureUser(requestPayload)
            .StatusCode(HttpStatusCode.Created)
            .ValidateSchema("Schemas/user_schema.json")
            .Body("$.name", NHamcrest.Is.EqualTo("Jane Doe"));
    }
}
```

---

## 📊 Viewing HTML Test Reports

When tests run, ExtentReports writes interactive test execution reports to the binary output directory:

```text
tests/Core.Framework.XunitTest/bin/Debug/net10.0/TestResults/ExtentReport.html
```

Open `ExtentReport.html` in any browser to review pass/fail status, detailed logs, and exception stack traces.

---

## 🔄 CI/CD Pipeline Integration (GitHub Actions)

Example workflow file `.github/workflows/api-tests.yml`:

```yaml
name: API Automation Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout Code
        uses: actions/checkout@v4

      - name: Setup .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore Dependencies
        run: dotnet restore ApiAutomationFramework.sln

      - name: Build Solution
        run: dotnet build ApiAutomationFramework.sln --configuration Release --no-restore

      - name: Run REST API Tests
        run: dotnet test ApiAutomationFramework.sln --configuration Release --no-build --logger "trx;LogFileName=test_results.trx"

      - name: Upload Extent HTML Report
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: extent-test-report
          path: "**/TestResults/ExtentReport.html"
```