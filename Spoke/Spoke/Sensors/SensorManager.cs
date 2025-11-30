using Spoke.Meta;
using IXICore;
using IXICore.Meta;

namespace Spoke.Sensors;

/// <summary>
/// Main sensor manager that coordinates all sensor data collection
/// </summary>
public class SensorManager
{
    private static SensorManager? _instance;
    public static SensorManager Instance => _instance ??= new SensorManager();

    public LocationSensorManager LocationSensor { get; } = new();
    public BatterySensorManager BatterySensor { get; } = new();
    public NetworkSensorManager NetworkSensor { get; } = new();

    private SensorManager()
    {
        Logging.info("SensorManager initialized");
    }

    public void StartAllSensors()
    {
        Logging.info("Starting all sensors");

        // Load settings and enable sensors
        var preferences = Preferences.Default;

        LocationSensor.IsEnabled = preferences.Get("sensor_location_enabled", false);
        BatterySensor.IsEnabled = preferences.Get("sensor_battery_enabled", true); // Default enabled
        NetworkSensor.IsEnabled = preferences.Get("sensor_network_enabled", true); // Default enabled

        Logging.info("All sensors started");
    }

    public void StopAllSensors()
    {
        Logging.info("Stopping all sensors");

        LocationSensor.IsEnabled = false;
        BatterySensor.IsEnabled = false;
        NetworkSensor.IsEnabled = false;

        Logging.info("All sensors stopped");
    }

    public void SaveSensorSettings()
    {
        var preferences = Preferences.Default;

        preferences.Set("sensor_location_enabled", LocationSensor.IsEnabled);
        preferences.Set("sensor_battery_enabled", BatterySensor.IsEnabled);
        preferences.Set("sensor_network_enabled", NetworkSensor.IsEnabled);

        Logging.info("Sensor settings saved");
    }
}

