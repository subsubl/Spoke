using CommunityToolkit.Mvvm.ComponentModel;
using Spoke.Sensors;
using System.Collections.ObjectModel;

namespace Spoke.Widgets;

public partial class SensorWidgetView : ContentPage
{
    public SensorWidgetView()
    {
        InitializeComponent();
        BindingContext = new SensorWidgetViewModel();
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        // Close the widget window
        WidgetManager.Instance.CloseWidget(this);
    }
}

public class SensorWidgetViewModel : ObservableObject
{
    public ObservableCollection<SensorDataItem> SensorData { get; } = new();

    public SensorWidgetViewModel()
    {
        LoadSensorData();
    }

    private void LoadSensorData()
    {
        // Load sensor data from individual sensor managers
        if (SensorManager.Instance != null)
        {
            // Battery sensor
            var batterySensor = SensorManager.Instance.BatterySensor;
            if (batterySensor.IsEnabled && batterySensor.LastBatteryInfo != null)
            {
                double percentage = 0;
                if (batterySensor.LastBatteryInfo.FullChargeCapacityInMilliwattHours > 0)
                {
                    percentage = (double)batterySensor.LastBatteryInfo.RemainingCapacityInMilliwattHours.Value /
                               batterySensor.LastBatteryInfo.FullChargeCapacityInMilliwattHours.Value * 100;
                }

                SensorData.Add(new SensorDataItem
                {
                    Name = "Battery",
                    Value = $"{percentage:F0}%"
                });
            }

            // Network sensor
            var networkSensor = SensorManager.Instance.NetworkSensor;
            if (networkSensor.IsEnabled)
            {
                SensorData.Add(new SensorDataItem
                {
                    Name = "Network",
                    Value = networkSensor.LastNetworkInfo?.GetNetworkConnectivityLevel().ToString() ?? "Unknown"
                });
            }

            // Location sensor
            var locationSensor = SensorManager.Instance.LocationSensor;
            if (locationSensor.IsEnabled && locationSensor.LastLocation != null)
            {
                SensorData.Add(new SensorDataItem
                {
                    Name = "Location",
                    Value = $"{locationSensor.LastLocation.Coordinate.Latitude:F2}, {locationSensor.LastLocation.Coordinate.Longitude:F2}"
                });
            }
        }
    }
}

public class SensorDataItem
{
    public string Name { get; set; }
    public string Value { get; set; }
}

