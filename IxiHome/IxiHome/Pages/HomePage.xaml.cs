using IxiHome.Data;
using IxiHome.Sensors;
using Windows.Devices.Power;
using Windows.System.Power;

namespace IxiHome.Pages;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        LoadEntities();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadEntities();
        UpdateSensorLabels();
        Device.StartTimer(TimeSpan.FromSeconds(5), () =>
        {
            UpdateSensorLabels();
            return true; // Continue timer
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Timer will stop automatically when page disappears
    }

    private async void LoadEntities()
    {
        await EntityManager.Instance.LoadEntitiesAsync();
        EntitiesCollection.ItemsSource = EntityManager.Instance.Entities;
    }

    private void UpdateSensorLabels()
    {
        // Update location sensor
        if (SensorManager.Instance.LocationSensor.IsEnabled && SensorManager.Instance.LocationSensor.LastLocation != null)
        {
            var loc = SensorManager.Instance.LocationSensor.LastLocation;
            var coord = loc.Coordinate.Point.Position;
            LocationSensorLabel.Text = $"{coord.Latitude:F4}, {coord.Longitude:F4}";
        }
        else
        {
            LocationSensorLabel.Text = SensorManager.Instance.LocationSensor.IsEnabled ? "Waiting..." : "Disabled";
        }

        // Update battery sensor
        if (SensorManager.Instance.BatterySensor.IsEnabled)
        {
            var battery = SensorManager.Instance.BatterySensor.LastBatteryInfo;
            if (battery != null)
            {
                double percentage = 0;
                if ((battery.FullChargeCapacityInMilliwattHours ?? 0) > 0)
                {
                    percentage = (double)(battery.RemainingCapacityInMilliwattHours ?? 0) / (double)(battery.FullChargeCapacityInMilliwattHours ?? 1) * 100;
                }
                string charging = battery.Status == BatteryStatus.Charging ? "⚡" : "";
                BatterySensorLabel.Text = $"{percentage:F0}% {charging}";
            }
            else
            {
                BatterySensorLabel.Text = "Waiting...";
            }
        }
        else
        {
            BatterySensorLabel.Text = "Disabled";
        }

        // Update network sensor
        if (SensorManager.Instance.NetworkSensor.IsEnabled)
        {
            var network = SensorManager.Instance.NetworkSensor.LastNetworkInfo;
            if (network != null)
            {
                string connectionType = network.GetNetworkConnectivityLevel().ToString();
                NetworkSensorLabel.Text = $"{connectionType}";
            }
            else
            {
                NetworkSensorLabel.Text = "Waiting...";
            }
        }
        else
        {
            NetworkSensorLabel.Text = "Disabled";
        }
    }

    private async void OnAddEntityClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("addentity");
    }
}
