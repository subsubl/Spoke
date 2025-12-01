namespace Spoke.Data.Models;

/// <summary>
/// Manages connection parameters for the QuIXI bridge
/// </summary>
public class QuixiConnection
{
    /// <summary>
    /// Bridge server address
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Bridge server port
    /// </summary>
    public int Port { get; set; } = 8001;

    /// <summary>
    /// Authentication username
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Authentication password (should be encrypted)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Current connection status
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Timestamp of last successful connection
    /// </summary>
    public DateTime LastConnected { get; set; }

    /// <summary>
    /// Validate that the connection has required parameters
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Host) &&
               Port > 0 && Port <= 65535;
    }

    /// <summary>
    /// Check if authentication is configured
    /// </summary>
    public bool HasAuthentication()
    {
        return !string.IsNullOrWhiteSpace(Username) &&
               !string.IsNullOrWhiteSpace(Password);
    }

    public override string ToString() => $"{Host}:{Port} ({(IsConnected ? "Connected" : "Disconnected")})";
}