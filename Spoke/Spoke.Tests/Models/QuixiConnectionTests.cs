using Xunit;
using Spoke.Data.Models;
using System;

namespace Spoke.Tests.Models;

public class QuixiConnectionTests
{
    [Fact]
    public void QuixiConnection_Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var connection = new QuixiConnection();

        // Assert
        Assert.Null(connection.Host);
        Assert.Equal(8001, connection.Port);
        Assert.Null(connection.Username);
        Assert.Null(connection.Password);
        Assert.False(connection.IsConnected);
        Assert.Equal(DateTime.MinValue, connection.LastConnected);
    }

    [Fact]
    public void QuixiConnection_CustomPort_SetsPortCorrectly()
    {
        // Arrange
        var connection = new QuixiConnection
        {
            Port = 9001
        };

        // Assert
        Assert.Equal(9001, connection.Port);
    }

    [Fact]
    public void QuixiConnection_ValidHost_ReturnsTrue()
    {
        // Arrange
        var connection = new QuixiConnection
        {
            Host = "192.168.1.100"
        };

        // Act & Assert - Basic validation would be in a separate validation method
        // For now, just test that the property is set
        Assert.Equal("192.168.1.100", connection.Host);
    }

    [Fact]
    public void QuixiConnection_ValidHostname_ReturnsTrue()
    {
        // Arrange
        var connection = new QuixiConnection
        {
            Host = "home-assistant.local"
        };

        // Act & Assert
        Assert.Equal("home-assistant.local", connection.Host);
    }
}