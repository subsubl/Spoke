using Xunit;
using Spoke.Data.Models;
using System;

namespace Spoke.Tests.Models;

public class WalletTests
{
    [Fact]
    public void Wallet_Constructor_CreatesEmptyWallet()
    {
        // Arrange & Act
        var wallet = new Wallet();

        // Assert
        Assert.NotNull(wallet.PublicKey);
        Assert.NotNull(wallet.EncryptedPrivateKey);
        Assert.Empty(wallet.Address);
        Assert.Null(wallet.Username);
        Assert.Null(wallet.ProfileImage);
        Assert.True(wallet.Created <= DateTime.UtcNow);
    }

    [Fact]
    public void Wallet_IsValid_ReturnsFalse_WhenKeysAreEmpty()
    {
        // Arrange
        var wallet = new Wallet();

        // Act
        var isValid = wallet.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Wallet_IsValid_ReturnsTrue_WhenKeysHaveMinimumLength()
    {
        // Arrange
        var wallet = new Wallet
        {
            PublicKey = new byte[32], // 32 bytes minimum
            EncryptedPrivateKey = new byte[32] // 32 bytes minimum
        };

        // Act
        var isValid = wallet.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Wallet_IsValid_ReturnsFalse_WhenPublicKeyTooShort()
    {
        // Arrange
        var wallet = new Wallet
        {
            PublicKey = new byte[16], // Too short
            EncryptedPrivateKey = new byte[32]
        };

        // Act
        var isValid = wallet.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Wallet_IsValid_ReturnsFalse_WhenPrivateKeyTooShort()
    {
        // Arrange
        var wallet = new Wallet
        {
            PublicKey = new byte[32],
            EncryptedPrivateKey = new byte[16] // Too short
        };

        // Act
        var isValid = wallet.IsValid();

        // Assert
        Assert.False(isValid);
    }
}