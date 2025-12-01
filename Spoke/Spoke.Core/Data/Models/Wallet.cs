using System.Security.Cryptography;

namespace Spoke.Data.Models;

/// <summary>
/// Represents a cryptographic wallet for blockchain-based identity
/// </summary>
public class Wallet
{
    /// <summary>
    /// Public key for identity verification
    /// </summary>
    public byte[] PublicKey { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Encrypted private key for signing
    /// </summary>
    public byte[] EncryptedPrivateKey { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Derived blockchain address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable identifier
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Base64-encoded avatar image
    /// </summary>
    public string? ProfileImage { get; set; }

    /// <summary>
    /// Wallet creation timestamp
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validate that the wallet has valid cryptographic keys
    /// </summary>
    public bool IsValid()
    {
        if (PublicKey.Length == 0 || EncryptedPrivateKey.Length == 0)
            return false;

        // TODO: Implement proper key validation using Ixian-Core crypto
        // For now, basic length checks
        return PublicKey.Length >= 32 && EncryptedPrivateKey.Length >= 32;
    }

    /// <summary>
    /// Check if address matches the derived address from public key
    /// </summary>
    public bool IsAddressValid()
    {
        if (string.IsNullOrEmpty(Address) || PublicKey.Length == 0)
            return false;

        // TODO: Implement address derivation and validation using Ixian-Core
        // For now, return true if address is not empty
        return !string.IsNullOrWhiteSpace(Address);
    }

    /// <summary>
    /// Verify that private key is properly encrypted
    /// </summary>
    public bool IsPrivateKeyEncrypted()
    {
        // TODO: Implement encryption validation
        // For now, assume it's encrypted if length > 0
        return EncryptedPrivateKey.Length > 0;
    }

    public override string ToString() => $"{Username ?? "Anonymous"} ({Address})";
}