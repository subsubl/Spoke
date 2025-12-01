using Spoke.Meta;
using Spoke.Network;
using Spoke.Sensors;
using Microsoft.Maui.Storage;

namespace Spoke.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        QuixiAddressEntry.Text = Config.quixiAddress;
        QuixiPortEntry.Text = Config.quixiApiPort.ToString();
        QuixiUsernameEntry.Text = Config.quixiUsername;
        HomeAssistantUrlEntry.Text = Config.homeAssistantUrl;
        DarkThemeSwitch.IsToggled = Config.useDarkTheme;
        NotificationsSwitch.IsToggled = Config.enableNotifications;
        AuthSwitch.IsToggled = Config.requireAuthentication;

        // Load sensor settings
        LocationSensorSwitch.IsToggled = Preferences.Default.Get("sensor_location_enabled", false);
        BatterySensorSwitch.IsToggled = Preferences.Default.Get("sensor_battery_enabled", true);
        NetworkSensorSwitch.IsToggled = Preferences.Default.Get("sensor_network_enabled", true);
    }

    private async void OnTestConnectionClicked(object sender, EventArgs e)
    {
        ConnectionStatusLabel.Text = ConnectionStatusMessages.ConnectingToIxianNetwork;
        ConnectionStatusLabel.TextColor = Colors.Gray;

        try
        {
            if (Node.Instance.quixiClient == null)
            {
                ConnectionStatusLabel.Text = "❌ QuIXI client not initialized";
                ConnectionStatusLabel.TextColor = Colors.Red;
                return;
            }

            bool success = await Node.Instance.quixiClient.TestConnectionAsync();
            
            if (success)
            {
                ConnectionStatusLabel.Text = ConnectionStatusMessages.ConnectedToIxianNetwork;
                ConnectionStatusLabel.TextColor = Colors.Green;
            }
            else
            {
                ConnectionStatusLabel.Text = "❌ Connection failed";
                ConnectionStatusLabel.TextColor = Colors.Red;
            }
        }
        catch (Exception ex)
        {
            ConnectionStatusLabel.Text = $"❌ Error: {ex.Message}";
            ConnectionStatusLabel.TextColor = Colors.Red;
        }
    }

    private void OnDarkThemeToggled(object sender, ToggledEventArgs e)
    {
        Application.Current!.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
    }

    private void OnLocationSensorToggled(object sender, ToggledEventArgs e)
    {
#if WINDOWS
        SensorManager.Instance.LocationSensor.IsEnabled = e.Value;
        SensorManager.Instance.SaveSensorSettings();
#endif
    }

    private void OnBatterySensorToggled(object sender, ToggledEventArgs e)
    {
#if WINDOWS
        SensorManager.Instance.BatterySensor.IsEnabled = e.Value;
        SensorManager.Instance.SaveSensorSettings();
#endif
    }

    private void OnNetworkSensorToggled(object sender, ToggledEventArgs e)
    {
#if WINDOWS
        SensorManager.Instance.NetworkSensor.IsEnabled = e.Value;
        SensorManager.Instance.SaveSensorSettings();
#endif
    }

    private void OnNotificationsToggled(object sender, ToggledEventArgs e)
    {
#if WINDOWS
        Notifications.NotificationManager.Instance.NotificationsEnabled = e.Value;
#endif
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Save QuIXI settings
        Config.quixiAddress = QuixiAddressEntry.Text ?? "";
        if (int.TryParse(QuixiPortEntry.Text, out int port))
            Config.quixiApiPort = port;
        Config.quixiUsername = QuixiUsernameEntry.Text ?? "";
        if (!string.IsNullOrEmpty(QuixiPasswordEntry.Text))
            Config.quixiPassword = QuixiPasswordEntry.Text;

        // Save Home Assistant settings
        Config.homeAssistantUrl = HomeAssistantUrlEntry.Text ?? "";
        if (!string.IsNullOrEmpty(HomeAssistantTokenEntry.Text))
            Config.homeAssistantToken = HomeAssistantTokenEntry.Text;

        // Save app settings
        Config.useDarkTheme = DarkThemeSwitch.IsToggled;
        Config.enableNotifications = NotificationsSwitch.IsToggled;
        Config.requireAuthentication = AuthSwitch.IsToggled;

        // Persist to storage
        Config.Save();

        // Initialize node if not already done
        if (!string.IsNullOrEmpty(Config.quixiAddress) && Node.Instance.quixiClient == null)
        {
            await App.InitializeNodeAsync();
        }
        else
        {
            // Reconnect QuIXI if settings changed
            await Node.Instance.ReconnectQuixiAsync();
        }

        await DisplayAlert("Settings Saved", "Your settings have been saved successfully.", "OK");
    }

    private async void OnExportViewingWalletClicked(object sender, EventArgs e)
    {
        try
        {
            // Export view-only wallet via adapter
            string? exportedPath = Spoke.Wallet.WalletAdapter.ExportViewingWallet();
            if (string.IsNullOrEmpty(exportedPath))
            {
                await DisplayAlert("Export Failed", "Unable to export viewing wallet.", "OK");
                return;
            }

            // Attempt to share/save the exported file
            bool shared = await Spoke.Platforms.SFileOperations.share(exportedPath, "Export Viewing Wallet");
            if (shared)
            {
                await DisplayAlert("Exported", $"Viewing wallet exported to:\n{exportedPath}", "OK");
            }
            else
            {
                await DisplayAlert("Export Saved", $"Viewing wallet saved to:\n{exportedPath}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Exception exporting viewing wallet: {ex.Message}", "OK");
        }
    }
}


