using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using IXICore;

namespace Spoke.Platforms.Android;

/// <summary>
/// Android notification service for entity state updates
/// </summary>
public class NotificationService
{
    private const string CHANNEL_ID = "ixihome_notifications";
    private const string CHANNEL_NAME = "IxiHome";
    private const string CHANNEL_DESCRIPTION = "Smart home notifications";
    private readonly Context _context;
    private readonly NotificationManager? _notificationManager;
    
    public NotificationService(Context context)
    {
        _context = context;
        _notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
        CreateNotificationChannel();
    }
    
    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(CHANNEL_ID, CHANNEL_NAME, NotificationImportance.Default)
            {
                Description = CHANNEL_DESCRIPTION
            };
            _notificationManager?.CreateNotificationChannel(channel);
        }
    }
    
    public void ShowNotification(string title, string message)
    {
        try
        {
            var intent = _context.PackageManager?.GetLaunchIntentForPackage(_context.PackageName ?? "");
            var pendingIntent = PendingIntent.GetActivity(_context, 0, intent, PendingIntentFlags.Immutable);
            
            var builder = new NotificationCompat.Builder(_context, CHANNEL_ID)
                .SetSmallIcon(Resource.Drawable.dotnet_bot)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetPriority(NotificationCompat.PriorityDefault)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);
            
            _notificationManager?.Notify(DateTime.Now.Millisecond, builder.Build());
        }
        catch (Exception ex)
        {
            Logging.error($"Error showing notification: {ex.Message}");
        }
    }
}


