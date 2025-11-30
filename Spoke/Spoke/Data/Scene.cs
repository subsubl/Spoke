using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace Spoke.Data;

/// <summary>
/// Represents a Home Assistant scene - a collection of entity states
/// </summary>
public partial class Scene : ObservableObject
{
    [ObservableProperty]
    private string _id = "";

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _icon = "mdi:palette";

    [ObservableProperty]
    private bool _isActive = false;

    [ObservableProperty]
    private DateTime _lastActivated = DateTime.MinValue;

    /// <summary>
    /// Entity states that make up this scene
    /// Key: entity_id, Value: desired state
    /// </summary>
    [JsonProperty("entities")]
    public Dictionary<string, SceneEntityState> Entities { get; set; } = new();

    /// <summary>
    /// Add an entity state to the scene
    /// </summary>
    public void AddEntityState(string entityId, string state, Dictionary<string, object>? attributes = null)
    {
        Entities[entityId] = new SceneEntityState
        {
            State = state,
            Attributes = attributes ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Remove an entity from the scene
    /// </summary>
    public void RemoveEntityState(string entityId)
    {
        Entities.Remove(entityId);
    }

    /// <summary>
    /// Get the state for a specific entity
    /// </summary>
    public SceneEntityState? GetEntityState(string entityId)
    {
        return Entities.TryGetValue(entityId, out var state) ? state : null;
    }
}

/// <summary>
/// Represents the state of an entity within a scene
/// </summary>
public class SceneEntityState
{
    [JsonProperty("state")]
    public string State { get; set; } = "";

    [JsonProperty("attributes")]
    public Dictionary<string, object> Attributes { get; set; } = new();
}

