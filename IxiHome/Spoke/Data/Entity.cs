using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace Spoke.Data;

/// <summary>
/// Base class for all Home Assistant entities
/// </summary>
public abstract partial class Entity : ObservableObject
{
    [ObservableProperty]
    private string _id = "";

    [ObservableProperty]
    private string _entityId = ""; // Home Assistant entity_id

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _domain = ""; // light, switch, sensor, etc.

    [ObservableProperty]
    private string _state = "";

    [ObservableProperty]
    private string _icon = "";

    [ObservableProperty]
    private string _displayType = ""; // toggle, gauge, graph, sensor

    [ObservableProperty]
    private int _order = 0;

    [ObservableProperty]
    private bool _isAvailable = true;

    [ObservableProperty]
    private DateTime _lastUpdated = DateTime.Now;

    [JsonProperty("attributes")]
    public Dictionary<string, object> Attributes { get; set; } = new();

    [JsonProperty("config")]
    public Dictionary<string, string> Config { get; set; } = new(); // Widget-specific configuration

    /// <summary>
    /// Update entity state from Home Assistant
    /// </summary>
    public virtual void UpdateState(string state, Dictionary<string, object> attributes)
    {
        State = state;
        Attributes = attributes ?? new Dictionary<string, object>();
        LastUpdated = DateTime.Now;
        IsAvailable = state != "unavailable";
    }

    /// <summary>
    /// Update entity state from websocket JsonElement
    /// </summary>
    public virtual void UpdateState(System.Text.Json.JsonElement jsonState)
    {
        string state = jsonState.GetProperty("state").GetString() ?? "";
        var attributes = new Dictionary<string, object>();

        if (jsonState.TryGetProperty("attributes", out var attributesElement))
        {
            foreach (var property in attributesElement.EnumerateObject())
            {
                attributes[property.Name] = property.Value.ToString(); // Simple conversion for now
            }
        }

        UpdateState(state, attributes);
    }

    /// <summary>
    /// Get display-friendly state string
    /// </summary>
    public virtual string GetDisplayState()
    {
        return State;
    }
}

/// <summary>
/// Toggle entity (switches, lights, etc.)
/// </summary>
public partial class ToggleEntity : Entity
{
    [ObservableProperty]
    private bool _isOn = false;

    public override void UpdateState(string state, Dictionary<string, object> attributes)
    {
        base.UpdateState(state, attributes);
        IsOn = state.ToLower() == "on";
    }

    public override string GetDisplayState()
    {
        return IsOn ? "ON" : "OFF";
    }
}

/// <summary>
/// Sensor entity (temperature, humidity, etc.)
/// </summary>
public partial class SensorEntity : Entity
{
    [ObservableProperty]
    private double _value = 0;

    [ObservableProperty]
    private string _unit = "";

    public override void UpdateState(string state, Dictionary<string, object> attributes)
    {
        base.UpdateState(state, attributes);
        
        if (double.TryParse(state, out double val))
        {
            Value = val;
        }

        if (attributes.ContainsKey("unit_of_measurement"))
        {
            Unit = attributes["unit_of_measurement"]?.ToString() ?? "";
        }
    }

    public override string GetDisplayState()
    {
        return $"{Value:F1} {Unit}";
    }
}

/// <summary>
/// Light entity with brightness and color support
/// </summary>
public partial class LightEntity : ToggleEntity
{
    [ObservableProperty]
    private int _brightness = 0; // 0-255

    [ObservableProperty]
    private int _colorR = 255;

    [ObservableProperty]
    private int _colorG = 255;

    [ObservableProperty]
    private int _colorB = 255;

    public override void UpdateState(string state, Dictionary<string, object> attributes)
    {
        base.UpdateState(state, attributes);

        if (attributes.ContainsKey("brightness"))
        {
            if (int.TryParse(attributes["brightness"]?.ToString(), out int brightness))
            {
                Brightness = brightness;
            }
        }

        if (attributes.ContainsKey("rgb_color"))
        {
            try
            {
                var rgb = attributes["rgb_color"] as Newtonsoft.Json.Linq.JArray;
                if (rgb != null && rgb.Count >= 3)
                {
                    ColorR = rgb[0].ToObject<int>();
                    ColorG = rgb[1].ToObject<int>();
                    ColorB = rgb[2].ToObject<int>();
                }
            }
            catch { }
        }
    }

    public override string GetDisplayState()
    {
        if (!IsOn) return "OFF";
        return $"ON ({Brightness * 100 / 255}%)";
    }
}

/// <summary>
/// Climate entity for temperature control
/// </summary>
public partial class ClimateEntity : Entity
{
    [ObservableProperty]
    private double _currentTemperature = 20.0;

    [ObservableProperty]
    private double _targetTemperature = 20.0;

    [ObservableProperty]
    private string _hvacMode = "off"; // off, heat, cool, auto, etc.

    [ObservableProperty]
    private string _hvacAction = "idle"; // idle, heating, cooling

    [ObservableProperty]
    private double _minTemp = 15.0;

    [ObservableProperty]
    private double _maxTemp = 30.0;

    public override void UpdateState(string state, Dictionary<string, object> attributes)
    {
        base.UpdateState(state, attributes);
        HvacMode = state;

        if (attributes.ContainsKey("current_temperature"))
        {
            if (double.TryParse(attributes["current_temperature"]?.ToString(), out double temp))
            {
                CurrentTemperature = temp;
            }
        }

        if (attributes.ContainsKey("temperature"))
        {
            if (double.TryParse(attributes["temperature"]?.ToString(), out double temp))
            {
                TargetTemperature = temp;
            }
        }

        if (attributes.ContainsKey("hvac_action"))
        {
            HvacAction = attributes["hvac_action"]?.ToString() ?? "idle";
        }

        if (attributes.ContainsKey("min_temp"))
        {
            if (double.TryParse(attributes["min_temp"]?.ToString(), out double temp))
            {
                MinTemp = temp;
            }
        }

        if (attributes.ContainsKey("max_temp"))
        {
            if (double.TryParse(attributes["max_temp"]?.ToString(), out double temp))
            {
                MaxTemp = temp;
            }
        }
    }

    public override string GetDisplayState()
    {
        return $"{CurrentTemperature:F1}°C → {TargetTemperature:F1}°C ({HvacMode})";
    }
}

/// <summary>
/// Gauge entity for visual display of numeric values
/// </summary>
public partial class GaugeEntity : SensorEntity
{
    [ObservableProperty]
    private double _minValue = 0;

    [ObservableProperty]
    private double _maxValue = 100;

    [ObservableProperty]
    private string _gaugeType = "circular"; // circular, linear

    [ObservableProperty]
    private string _colorScheme = "blue"; // blue, green, red, orange, custom

    public double GetPercentage()
    {
        if (MaxValue == MinValue) return 0;
        return ((Value - MinValue) / (MaxValue - MinValue)) * 100;
    }
}

/// <summary>
/// Graph entity for displaying historical data
/// </summary>
public partial class GraphEntity : SensorEntity
{
    [ObservableProperty]
    private string _graphType = "line"; // line, bar, area

    [ObservableProperty]
    private int _historyHours = 24;

    [ObservableProperty]
    private List<DataPoint> _dataPoints = new();

    public void AddDataPoint(double value, DateTime timestamp)
    {
        DataPoints.Add(new DataPoint { Value = value, Timestamp = timestamp });

        // Keep only recent data based on historyHours
        var cutoffTime = DateTime.Now.AddHours(-HistoryHours);
        DataPoints = DataPoints.Where(dp => dp.Timestamp >= cutoffTime).ToList();
    }
}

/// <summary>
/// Represents a single data point in a graph
/// </summary>
public class DataPoint
{
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
}


