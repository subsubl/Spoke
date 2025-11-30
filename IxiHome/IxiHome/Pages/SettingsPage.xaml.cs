using IxiHome.Meta;
using IxiHome.Sensors;
using Microsoft.Maui.Storage;

namespace IxiHome.Pages;

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
        ConnectionStatusLabel.Text = "Testing connection...";
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
                ConnectionStatusLabel.Text = "✓ Connection successful";
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
        SensorManager.Instance.LocationSensor.IsEnabled = e.Value;
        SensorManager.Instance.SaveSensorSettings();
    }

    private void OnBatterySensorToggled(object sender, ToggledEventArgs e)
    {
        SensorManager.Instance.BatterySensor.IsEnabled = e.Value;
        SensorManager.Instance.SaveSensorSettings();
    }

    private void OnNetworkSensorToggled(object sender, ToggledEventArgs e)
    {
        SensorManager.Instance.NetworkSensor.IsEnabled = e.Value;
        SensorManager.Instance.SaveSensorSettings();
    }

    private void OnNotificationsToggled(object sender, ToggledEventArgs e)
    {
        Notifications.NotificationManager.Instance.NotificationsEnabled = e.Value;
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
}
