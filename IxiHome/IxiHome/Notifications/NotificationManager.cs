using IxiHome.Meta;
using IXICore;
using IXICore.Meta;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace IxiHome.Notifications;

/// <summary>
/// Manages Windows notifications for entity changes and sensor alerts
/// </summary>
public class NotificationManager
{
    private static NotificationManager? _instance;
    public static NotificationManager Instance => _instance ??= new NotificationManager();

    private bool _notificationsEnabled = true;

    public bool NotificationsEnabled
    {
        get => _notificationsEnabled;
        set
        {
            _notificationsEnabled = value;
            if (_notificationsEnabled)
            {
                Logging.info("Notifications enabled");
            }
            else
            {
                Logging.info("Notifications disabled");
            }
        }
    }

    private NotificationManager()
    {
        Logging.info("NotificationManager initialized");
    }

    public void ShowEntityStateChangedNotification(string entityName, string newState, string? friendlyName = null)
    {
        if (!NotificationsEnabled) return;

        try
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = $"{friendlyName ?? entityName} Changed"
                            },
                            new AdaptiveText()
                            {
                                Text = $"State: {newState}"
                            }
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("View", $"action=view&entity={entityName}")
                        {
                            ActivationType = ToastActivationType.Foreground
                        }
                    }
                },
                Launch = $"entity={entityName}"
            };

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);

            Logging.info($"Notification shown for entity {entityName} state change to {newState}");
        }
        catch (Exception ex)
        {
            Logging.error($"Failed to show entity notification: {ex.Message}");
        }
    }

    public void ShowSensorAlertNotification(string sensorType, string message, string? details = null)
    {
        if (!NotificationsEnabled) return;

        try
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = $"{sensorType} Alert"
                            },
                            new AdaptiveText()
                            {
                                Text = message
                            }
                        }
                    }
                },
                Launch = $"sensor={sensorType}"
            };

            if (!string.IsNullOrEmpty(details))
            {
                toastContent.Visual.BindingGeneric.Children.Add(new AdaptiveText() { Text = details });
            }

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);

            Logging.info($"Sensor alert notification shown: {sensorType} - {message}");
        }
        catch (Exception ex)
        {
            Logging.error($"Failed to show sensor alert notification: {ex.Message}");
        }
    }

    public void ShowSystemNotification(string title, string message, string? launchArgs = null)
    {
        if (!NotificationsEnabled) return;

        try
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title
                            },
                            new AdaptiveText()
                            {
                                Text = message
                            }
                        }
                    }
                },
                Launch = launchArgs ?? ""
            };

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);

            Logging.info($"System notification shown: {title}");
        }
        catch (Exception ex)
        {
            Logging.error($"Failed to show system notification: {ex.Message}");
        }
    }
}