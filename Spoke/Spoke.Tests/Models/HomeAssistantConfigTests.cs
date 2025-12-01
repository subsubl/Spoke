using Xunit;
using Spoke.Data.Models;

namespace Spoke.Tests.Models;

public class HomeAssistantConfigTests
{
    [Fact]
    public void HomeAssistantConfig_Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var config = new HomeAssistantConfig();

        // Assert
        Assert.Null(config.Url);
        Assert.Null(config.AccessToken);
        Assert.Null(config.WebSocketUrl);
        Assert.Equal(30, config.SyncInterval);
        Assert.False(config.IsConfigured);
    }

    [Fact]
    public void HomeAssistantConfig_ValidUrl_SetsUrlCorrectly()
    {
        // Arrange
        var config = new HomeAssistantConfig
        {
            Url = "http://home-assistant.local:8123"
        };

        // Assert
        Assert.Equal("http://home-assistant.local:8123", config.Url);
    }

    [Fact]
    public void HomeAssistantConfig_ValidHttpsUrl_SetsUrlCorrectly()
    {
        // Arrange
        var config = new HomeAssistantConfig
        {
            Url = "https://home-assistant.example.com"
        };

        // Assert
        Assert.Equal("https://home-assistant.example.com", config.Url);
    }

    [Fact]
    public void HomeAssistantConfig_CustomSyncInterval_SetsCorrectly()
    {
        // Arrange
        var config = new HomeAssistantConfig
        {
            SyncInterval = 60
        };

        // Assert
        Assert.Equal(60, config.SyncInterval);
    }

    [Fact]
    public void HomeAssistantConfig_IsConfigured_ReturnsFalse_WhenUrlIsNull()
    {
        // Arrange
        var config = new HomeAssistantConfig();

        // Assert
        Assert.False(config.IsConfigured);
    }

    [Fact]
    public void HomeAssistantConfig_IsConfigured_ReturnsFalse_WhenAccessTokenIsNull()
    {
        // Arrange
        var config = new HomeAssistantConfig
        {
            Url = "http://home-assistant.local:8123"
        };

        // Assert
        Assert.False(config.IsConfigured);
    }
}