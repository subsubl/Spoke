using System.Text.Json.Serialization;

namespace Spoke.Data.Models;

/// <summary>
/// Configuration for Home Assistant integration
/// </summary>
public class HomeAssistantConfig
{
    /// <summary>
    /// Base URL of the Home Assistant instance
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted long-lived access token
    /// </summary>
    public byte[] EncryptedAccessToken { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// WebSocket URL for real-time updates (optional, derived from BaseUrl if not set)
    /// </summary>
    public string? WebSocketUrl { get; set; }

    /// <summary>
    /// Home Assistant version
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// List of supported features
    /// </summary>
    public List<string> Features { get; set; } = new();

    /// <summary>
    /// Validate that the configuration is valid
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return false;

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var baseUri))
            return false;

        if (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps)
            return false;

        if (EncryptedAccessToken.Length == 0)
            return false;

        return true;
    }

    /// <summary>
    /// Get the WebSocket URL, deriving from BaseUrl if not explicitly set
    /// </summary>
    public string GetWebSocketUrl()
    {
        if (!string.IsNullOrWhiteSpace(WebSocketUrl))
            return WebSocketUrl;

        if (Uri.TryCreate(BaseUrl, UriKind.Absolute, out var baseUri))
        {
            var wsScheme = baseUri.Scheme == Uri.UriSchemeHttps ? "wss" : "ws";
            return $"{wsScheme}://{baseUri.Host}:{baseUri.Port}/api/websocket";
        }

        return string.Empty;
    }

    /// <summary>
    /// Check if the Home Assistant version is compatible
    /// </summary>
    public bool IsVersionCompatible()
    {
        if (string.IsNullOrWhiteSpace(Version))
            return false;

        // TODO: Implement version compatibility check
        // For now, assume versions starting with 2023+ are compatible
        return Version.StartsWith("2023") || Version.StartsWith("2024") || Version.StartsWith("2025");
    }

    /// <summary>
    /// Check if a specific feature is supported
    /// </summary>
    public bool SupportsFeature(string feature) =>
        Features.Contains(feature, StringComparer.OrdinalIgnoreCase);

    public override string ToString() => $"{BaseUrl} (v{Version ?? "unknown"})";
}