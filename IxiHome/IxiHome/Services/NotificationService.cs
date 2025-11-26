using IXICore;
using IXICore.Meta;

namespace IxiHome.Services;

/// <summary>
/// Cross-platform notification service wrapper
/// </summary>
public class NotificationService : INotificationService
{
    private static NotificationService? _instance;
    public static NotificationService Instance => _instance ??= new NotificationService();
    
#if ANDROID
    private Platforms.Android.NotificationService? _platformService;
#elif IOS
    private Platforms.iOS.NotificationService? _platformService;
#endif
    
    private NotificationService()
    {
        InitializePlatformService();
    }
    
    private void InitializePlatformService()
    {
#if ANDROID
        if (Platform.CurrentActivity != null)
        {
            _platformService = new Platforms.Android.NotificationService(Platform.CurrentActivity);
        }
#elif IOS
        _platformService = new Platforms.iOS.NotificationService();
#endif
    }
    
    public async Task ShowNotificationAsync(string title, string message)
    {
        try
        {
#if ANDROID
            _platformService?.ShowNotification(title, message);
            await Task.CompletedTask;
#elif IOS
            if (_platformService != null)
            {
                await _platformService.ShowNotificationAsync(title, message);
            }
#else
            Logging.info($"Notification: {title} - {message}");
            await Task.CompletedTask;
#endif
        }
        catch (Exception ex)
        {
            Logging.error($"Error showing notification: {ex.Message}");
        }
    }
    
    public async Task RequestPermissionAsync()
    {
        // iOS handles permission in constructor
        // Android requires runtime permission for notifications on API 33+
        await Task.CompletedTask;
    }
}
