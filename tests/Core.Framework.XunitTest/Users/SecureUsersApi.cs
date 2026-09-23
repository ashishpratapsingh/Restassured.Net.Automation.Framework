using System.Text.Json;
using Core.Framework.BaseApiObject;
using Core.Framework.Core.Execution;
using Core.Framework.XunitTest.Users.Models;
using RestAssured.Response;
using static RestAssured.Dsl;

namespace Core.Framework.XunitTest.Users;

public class SecureUsersApi : AuthenticatedApiObject
{
    private const string SecureProfileEndpoint = "/api/v2/user/profile";
    private const string UsersEndpoint = "/api/users";

    public VerifiableResponse GetUserProfile()
    {
        return ApiRequestExecutor.ExecuteSafe(
            () => Given()
                //.Spec(AuthenticatedRequestSpec)
                .Spec(RequestSpec)
                .When()
                .Get(SecureProfileEndpoint),
            //$"GET {SecureProfileEndpoint} [Authenticated]"
            $"GET {SecureProfileEndpoint}"
        );
    }

    public VerifiableResponse CreateSecureUser(CreateUserRequest requestPayload)
    {
        string jsonBody = JsonSerializer.Serialize(requestPayload);
        
        return ApiRequestExecutor.ExecuteSafe(
            () => Given()
                //.Spec(AuthenticatedRequestSpec)
                .Spec(RequestSpec)
                .ContentType("application/json")
                .Body(jsonBody)
                .When()
                .Post(UsersEndpoint),                                
            //$"POST {UsersEndpoint} [Authenticated]"
            $"POST {UsersEndpoint}"
        );
    }
}