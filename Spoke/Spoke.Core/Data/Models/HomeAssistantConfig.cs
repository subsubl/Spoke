namespace Spoke.Data.Models;

/// <summary>
/// Stores Home Assistant instance configuration
/// </summary>
public class HomeAssistantConfig
{
    /// <summary>
    /// Home Assistant base URL
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Long-lived access token
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// WebSocket API URL (computed from Url)
    /// </summary>
    public string? WebSocketUrl => ComputeWebSocketUrl();

    /// <summary>
    /// State sync frequency in seconds
    /// </summary>
    public int SyncInterval { get; set; } = 30;

    /// <summary>
    /// Whether configuration is complete and valid
    /// </summary>
    public bool IsConfigured => IsValid();

    /// <summary>
    /// Validate that the configuration is complete
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Url) &&
               !string.IsNullOrWhiteSpace(AccessToken) &&
               Uri.IsWellFormedUriString(Url, UriKind.Absolute);
    }

    /// <summary>
    /// Check if the URL is a valid HTTP/HTTPS URL
    /// </summary>
    public bool IsUrlValid()
    {
        if (string.IsNullOrWhiteSpace(Url))
            return false;

        return Uri.TryCreate(Url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Check if access token appears to be valid
    /// </summary>
    public bool IsAccessTokenValid()
    {
        return !string.IsNullOrWhiteSpace(AccessToken) &&
               AccessToken.Length > 10; // Basic length check
    }

    private string? ComputeWebSocketUrl()
    {
        if (string.IsNullOrWhiteSpace(Url))
            return null;

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
            return null;

        var wsScheme = uri.Scheme == Uri.UriSchemeHttps ? "wss" : "ws";
        return $"{wsScheme}://{uri.Host}:{uri.Port}/api/websocket";
    }

    public override string ToString() => $"{Url ?? "Not configured"}";
}