using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Spoke.Data.Models;

/// <summary>
/// Represents a Home Assistant entity that can be controlled through the app
/// </summary>
public class Entity : INotifyPropertyChanged
{
    private string _id = string.Empty;
    private string _name = string.Empty;
    private string _type = string.Empty;
    private string _state = string.Empty;
    private Dictionary<string, object> _attributes = new();
    private DateTime _lastUpdated;
    private bool _isAvailable = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>
    /// Human-readable display name
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Entity type (light, switch, sensor, etc.)
    /// </summary>
    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    /// <summary>
    /// Current state value
    /// </summary>
    public string State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    /// <summary>
    /// Additional properties as key-value pairs
    /// </summary>
    public Dictionary<string, object> Attributes
    {
        get => _attributes;
        set => SetProperty(ref _attributes, value);
    }

    /// <summary>
    /// Timestamp of last state change
    /// </summary>
    public DateTime LastUpdated
    {
        get => _lastUpdated;
        set => SetProperty(ref _lastUpdated, value);
    }

    /// <summary>
    /// Whether the entity is currently reachable
    /// </summary>
    public bool IsAvailable
    {
        get => _isAvailable;
        set => SetProperty(ref _isAvailable, value);
    }

    /// <summary>
    /// Get a typed attribute value
    /// </summary>
    public T? GetAttribute<T>(string key)
    {
        if (_attributes.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Set an attribute value
    /// </summary>
    public void SetAttribute(string key, object value)
    {
        _attributes[key] = value;
        OnPropertyChanged(nameof(Attributes));
    }

    /// <summary>
    /// Check if entity is of a specific type
    /// </summary>
    public bool IsType(string type) => string.Equals(_type, type, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Check if entity state matches a value
    /// </summary>
    public bool IsState(string state) => string.Equals(_state, state, StringComparison.OrdinalIgnoreCase);

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public override string ToString() => $"{_type}:{_id} ({_state})";
}