namespace Spoke.Services;

/// <summary>
/// Cross-platform notification service interface
/// </summary>
public interface INotificationService
{
    Task ShowNotificationAsync(string title, string message);
    Task RequestPermissionAsync();
}


