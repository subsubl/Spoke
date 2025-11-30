using CommunityToolkit.Mvvm.ComponentModel;
using IxiHome.Data;
using System.Collections.ObjectModel;

namespace Spoke.Widgets;

/// <summary>
/// Base class for all desktop widgets
/// </summary>
public abstract partial class Widget : ObservableObject
{
    [ObservableProperty]
    private string _id = "";

    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private double _x = 100;

    [ObservableProperty]
    private double _y = 100;

    [ObservableProperty]
    private double _width = 200;

    [ObservableProperty]
    private double _height = 150;

    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private bool _isLocked = false;

    /// <summary>
    /// The entity this widget controls (if any)
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Update the widget's data
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Get the widget's view
    /// </summary>
    public abstract ContentPage GetView();
}

/// <summary>
/// Widget for controlling a single entity (light, switch, etc.)
/// </summary>
public partial class EntityWidget : Widget
{
    [ObservableProperty]
    private Entity? _entity;

    public EntityWidget(string entityId)
    {
        EntityId = entityId;
        Id = $"entity_{entityId}";
        Title = "Entity Control";
        Width = 180;
        Height = 120;
    }

    public override void Update()
    {
        if (EntityId != null)
        {
            Entity = EntityManager.Instance.GetEntityByEntityId(EntityId);
        }
    }

    public override ContentPage GetView()
    {
        return new EntityWidgetView { BindingContext = this };
    }
}

/// <summary>
/// Widget for displaying sensor data
/// </summary>
public partial class SensorWidget : Widget
{
    [ObservableProperty]
    private ObservableCollection<SensorData> _sensorData = new();

    public SensorWidget()
    {
        Id = "sensor_widget";
        Title = "Sensors";
        Width = 250;
        Height = 200;
    }

    public override void Update()
    {
        // Update sensor data from managers
        SensorData.Clear();

        // Add battery data
        var batteryManager = Sensors.SensorManager.Instance.BatterySensor;
        if (batteryManager.IsEnabled && batteryManager.LastBatteryInfo != null)
        {
            double percentage = 0;
            if (batteryManager.LastBatteryInfo.FullChargeCapacityInMilliwattHours > 0)
            {
                percentage = (double)batteryManager.LastBatteryInfo.RemainingCapacityInMilliwattHours.Value /
                           batteryManager.LastBatteryInfo.FullChargeCapacityInMilliwattHours.Value * 100;
            }

            SensorData.Add(new SensorData
            {
                Name = "Battery",
                Value = $"{percentage:F0}%",
                Icon = "battery_icon.png"
            });
        }

        // Add network data
        var networkManager = Sensors.SensorManager.Instance.NetworkSensor;
        if (networkManager.IsEnabled)
        {
            SensorData.Add(new SensorData
            {
                Name = "Network",
                Value = networkManager.LastNetworkInfo?.GetNetworkConnectivityLevel().ToString() ?? "Unknown",
                Icon = "network_icon.png"
            });
        }
    }

    public override ContentPage GetView()
    {
        return new SensorWidgetView { BindingContext = this };
    }
}

/// <summary>
/// Widget for quick scene activation
/// </summary>
public partial class SceneWidget : Widget
{
    [ObservableProperty]
    private ObservableCollection<Scene> _scenes = new();

    public SceneWidget()
    {
        Id = "scene_widget";
        Title = "Scenes";
        Width = 200;
        Height = 150;
    }

    public override void Update()
    {
        Scenes.Clear();
        foreach (var scene in SceneManager.Instance.GetScenes().Take(4)) // Show max 4 scenes
        {
            Scenes.Add(scene);
        }
    }

    public override ContentPage GetView()
    {
        return new SceneWidgetView { BindingContext = this };
    }
}

/// <summary>
/// Represents sensor data for display
/// </summary>
public class SensorData
{
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public string Icon { get; set; } = "";
}

