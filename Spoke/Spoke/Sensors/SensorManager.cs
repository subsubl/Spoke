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

#if WINDOWS
    public LocationSensorManager LocationSensor { get; } = new();
    public BatterySensorManager BatterySensor { get; } = new();
    public NetworkSensorManager NetworkSensor { get; } = new();
#endif

    private SensorManager()
    {
        Logging.info("SensorManager initialized");
    }

    public void StartAllSensors()
    {
        Logging.info("Starting all sensors");

        // Load settings and enable sensors
        var preferences = Preferences.Default;

#if WINDOWS
        LocationSensor.IsEnabled = preferences.Get("sensor_location_enabled", false);
        BatterySensor.IsEnabled = preferences.Get("sensor_battery_enabled", true); // Default enabled
        NetworkSensor.IsEnabled = preferences.Get("sensor_network_enabled", true); // Default enabled
#endif

        Logging.info("All sensors started");
    }

    public void StopAllSensors()
    {
        Logging.info("Stopping all sensors");

#if WINDOWS
        LocationSensor.IsEnabled = false;
        BatterySensor.IsEnabled = false;
        NetworkSensor.IsEnabled = false;
#endif

        Logging.info("All sensors stopped");
    }

    public void SaveSensorSettings()
    {
        var preferences = Preferences.Default;

#if WINDOWS
        preferences.Set("sensor_location_enabled", LocationSensor.IsEnabled);
        preferences.Set("sensor_battery_enabled", BatterySensor.IsEnabled);
        preferences.Set("sensor_network_enabled", NetworkSensor.IsEnabled);
#endif

        Logging.info("Sensor settings saved");
    }
}

