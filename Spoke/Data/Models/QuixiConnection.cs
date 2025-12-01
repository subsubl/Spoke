using System.Net;

namespace Spoke.Data.Models;

/// <summary>
/// Represents a connection to a QuIXI bridge server
/// </summary>
public class QuixiConnection
{
    /// <summary>
    /// Bridge server hostname or IP address
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Bridge server port number
    /// </summary>
    public int Port { get; set; } = 8080;

    /// <summary>
    /// Whether to use secure connection (HTTPS/WSS)
    /// </summary>
    public bool IsSecure { get; set; } = true;

    /// <summary>
    /// Encrypted authentication token
    /// </summary>
    public byte[] EncryptedAuthToken { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Last successful connection timestamp
    /// </summary>
    public DateTime? LastConnected { get; set; }

    /// <summary>
    /// Current connection status
    /// </summary>
    public ConnectionStatus Status { get; set; } = ConnectionStatus.Disconnected;

    /// <summary>
    /// Validate that the connection configuration is valid
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Host))
            return false;

        if (Port < 1 || Port > 65535)
            return false;

        // Validate hostname/IP format
        if (!Uri.CheckHostName(Host) && !IPAddress.TryParse(Host, out _))
            return false;

        return true;
    }

    /// <summary>
    /// Get the full connection URI
    /// </summary>
    public Uri GetUri()
    {
        var scheme = IsSecure ? "wss" : "ws";
        return new Uri($"{scheme}://{Host}:{Port}");
    }

    /// <summary>
    /// Check if authentication token is present
    /// </summary>
    public bool HasAuthToken() => EncryptedAuthToken.Length > 0;

    public override string ToString() => $"{Host}:{Port} ({Status})";
}

/// <summary>
/// Connection status enumeration
/// </summary>
public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Authenticating,
    Authenticated,
    Error
}