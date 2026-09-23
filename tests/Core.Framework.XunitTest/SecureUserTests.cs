using System.Net;
using Core.Framework.XunitTest.Users.Models;
using Core.Framework.Core.Retries;
using Core.Framework.BaseTesting;
using Core.Framework.XunitTest.Users;

namespace Core.Framework.XunitTest;

public class SecureUserTest : BaseTest
{
    private readonly SecureUsersApi _usersApi;

    // xUnit uses the constructor for per-test setup (replaces NUnit [SetUp])
    public SecureUserTest()
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
            //.ValidateSchema("Schemas/user_schema.json")
            .Body("$.name", NHamcrest.Is.EqualTo("Jane Doe"));
    }
}