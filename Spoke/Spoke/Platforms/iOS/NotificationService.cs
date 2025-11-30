using UserNotifications;
using IXICore.Meta;

namespace Spoke.Platforms.iOS;

/// <summary>
/// iOS notification service for entity state updates
/// </summary>
public class NotificationService
{
    private readonly UNUserNotificationCenter _notificationCenter;
    
    public NotificationService()
    {
        _notificationCenter = UNUserNotificationCenter.Current;
        RequestAuthorization();
    }
    
    private async void RequestAuthorization()
    {
        var (granted, error) = await _notificationCenter.RequestAuthorizationAsync(
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge);
        
        if (!granted)
        {
            Logging.warn("Notification authorization not granted");
        }
    }
    
    public async Task ShowNotificationAsync(string title, string message)
    {
        try
        {
            var content = new UNMutableNotificationContent
            {
                Title = title,
                Body = message,
                Sound = UNNotificationSound.Default
            };
            
            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
            var request = UNNotificationRequest.FromIdentifier(Guid.NewGuid().ToString(), content, trigger);
            
            await _notificationCenter.AddNotificationRequestAsync(request);
        }
        catch (Exception ex)
        {
            Logging.error($"Error showing notification: {ex.Message}");
        }
    }
}


