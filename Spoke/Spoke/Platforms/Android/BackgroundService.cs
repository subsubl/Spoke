using Android.App;
using Android.Content;
using IXICore;

namespace Spoke.Platforms.Android;

/// <summary>
/// Android background service for keeping the app alive
/// </summary>
[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync)]
public class BackgroundService : Service
{
    private const int SERVICE_NOTIFICATION_ID = 9999;
    private const string CHANNEL_ID = "ixihome_background";
    
    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }
    
    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        Logging.info("BackgroundService started");
        
        var notification = new Notification.Builder(this, CHANNEL_ID)
            .SetContentTitle("Spoke")
            .SetContentText("Monitoring your smart home")
            .SetSmallIcon(Resource.Drawable.dotnet_bot)
            .Build();
        
        StartForeground(SERVICE_NOTIFICATION_ID, notification);
        
        return StartCommandResult.Sticky;
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
        Logging.info("BackgroundService stopped");
    }
}


