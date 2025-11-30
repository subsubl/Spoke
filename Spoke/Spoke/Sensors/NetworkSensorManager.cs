using IxiHome.Meta;
using IXICore;
using IXICore.Meta;
using Windows.Networking.Connectivity;

namespace Spoke.Sensors;

/// <summary>
/// Manages network sensor data collection
/// </summary>
public class NetworkSensorManager
{
    private bool _isEnabled = false;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            if (_isEnabled)
            {
                StartNetworkMonitoring();
            }
            else
            {
                StopNetworkMonitoring();
            }
        }
    }

    public Windows.Networking.Connectivity.ConnectionProfile? LastNetworkInfo { get; private set; }

    private Windows.Networking.Connectivity.NetworkConnectivityLevel _lastConnectivityLevel = Windows.Networking.Connectivity.NetworkConnectivityLevel.None;

    public event EventHandler<Windows.Networking.Connectivity.ConnectionProfile>? NetworkUpdated;

    public NetworkSensorManager()
    {
        NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
    }

    private void OnNetworkStatusChanged(object? sender)
    {
        if (!_isEnabled) return;

        ReportNetworkStatus();
    }

    private void StartNetworkMonitoring()
    {
        Logging.info("Network monitoring started");
        ReportNetworkStatus();
    }

    private void StopNetworkMonitoring()
    {
        Logging.info("Network monitoring stopped");
    }

    private void ReportNetworkStatus()
    {
        try
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();

            if (profile != null)
            {
                LastNetworkInfo = profile;
                NetworkUpdated?.Invoke(this, profile);
                SendNetworkData(profile);

                // Check for connectivity changes
                CheckConnectivityChanges(profile);
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Network sensor error: {ex.Message}");
        }
    }

    private void CheckConnectivityChanges(Windows.Networking.Connectivity.ConnectionProfile profile)
    {
        try
        {
            var currentLevel = profile.GetNetworkConnectivityLevel();

            if (_lastConnectivityLevel != currentLevel)
            {
                string message = currentLevel switch
                {
                    Windows.Networking.Connectivity.NetworkConnectivityLevel.InternetAccess => "Internet connection restored",
                    Windows.Networking.Connectivity.NetworkConnectivityLevel.ConstrainedInternetAccess => "Limited internet access",
                    Windows.Networking.Connectivity.NetworkConnectivityLevel.LocalAccess => "Local network only",
                    Windows.Networking.Connectivity.NetworkConnectivityLevel.None => "No network connection",
                    _ => "Network status changed"
                };

                if (_lastConnectivityLevel == Windows.Networking.Connectivity.NetworkConnectivityLevel.None ||
                    currentLevel == Windows.Networking.Connectivity.NetworkConnectivityLevel.None)
                {
                    // Only show notifications for connection loss/gain, not other changes
                    Notifications.NotificationManager.Instance.ShowSensorAlertNotification(
                        "Network",
                        message);
                }

                _lastConnectivityLevel = currentLevel;
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Error checking connectivity changes: {ex.Message}");
        }
    }

    private async void SendNetworkData(Windows.Networking.Connectivity.ConnectionProfile profile)
    {
        if (Node.Instance.quixiClient == null) return;

        try
        {
            var data = new Dictionary<string, object>
            {
                ["connected"] = profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess,
                ["connection_type"] = profile.NetworkAdapter?.IanaInterfaceType.ToString() ?? "unknown",
                ["ssid"] = profile.WlanConnectionProfileDetails?.GetConnectedSsid() ?? "",
                ["signal_strength"] = 0 // profile.WlanConnectionProfileDetails?.GetSignalBars() ?? 0
            };

            await Node.Instance.quixiClient.SendCommandAsync("sensor.network", "update", data);
        }
        catch (Exception ex)
        {
            Logging.error($"Failed to send network data: {ex.Message}");
        }
    }

    public Windows.Networking.Connectivity.ConnectionProfile? GetCurrentNetworkProfile()
    {
        try
        {
            return NetworkInformation.GetInternetConnectionProfile();
        }
        catch (Exception ex)
        {
            Logging.error($"Failed to get network profile: {ex.Message}");
            return null;
        }
    }
}

