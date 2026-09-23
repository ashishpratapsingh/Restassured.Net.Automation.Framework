using System.Text.Json.Serialization;

namespace Core.Framework.XunitTest.Users.Models;

public record CreateUserRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("job")] string Job
);

public record UserResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Job { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}