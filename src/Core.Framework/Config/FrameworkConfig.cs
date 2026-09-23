using Microsoft.Extensions.Configuration;

namespace Core.Framework.Config;
public class FrameworkConfig
{
    private static readonly Lazy<FrameworkConfig> _instance = new(() => new FrameworkConfig());
    public static FrameworkConfig Instance => _instance.Value;

    public string BaseUrl { get; private set; } = string.Empty;
    public int TimeoutSeconds { get; private set; }
    public int MaxRetryCount { get; private set; }
    public List<string> EnabledReporters { get; private set; } = new();
    public string TokenUrl { get; private set; } = string.Empty;
    public string ClientId { get; private set; } = string.Empty;
    public string ClientSecret { get; private set; } = string.Empty;
    public string GrantType { get; private set; } = string.Empty;
    public string Scope { get; private set; } = string.Empty;

    private FrameworkConfig()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        BaseUrl = config["Environment:BaseUrl"] ?? "https://localhost";
        TimeoutSeconds = int.Parse(config["Environment:TimeoutSeconds"] ?? "30");
        MaxRetryCount = int.Parse(config["FrameworkOptions:MaxRetryCount"] ?? "1");
        EnabledReporters = config.GetSection("FrameworkOptions:EnabledReporters").Get<List<string>>() ?? new();
        TokenUrl = config["AuthOptions:TokenUrl"] ?? string.Empty;
        ClientId = config["AuthOptions:ClientId"] ?? string.Empty;
        ClientSecret = config["AuthOptions:ClientSecret"] ?? string.Empty;
        GrantType = config["AuthOptions:GrantType"] ?? "client_credentials";
        Scope = config["AuthOptions:Scope"] ?? string.Empty;
    }
}