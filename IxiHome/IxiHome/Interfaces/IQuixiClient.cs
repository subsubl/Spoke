using IxiHome.Data;

namespace IxiHome.Interfaces;

public interface IQuixiClient
{
    event EventHandler<EntityStateChangedEventArgs>? EntityStateChanged;
    event EventHandler<bool>? ConnectionStatusChanged;

    Task<bool> TestConnectionAsync();
    Task<List<Entity>> GetEntitiesAsync();
    Task<bool> SendCommandAsync(string entityId, string command, Dictionary<string, object>? parameters = null);
    Task<bool> TurnOnAsync(string entityId);
    Task<bool> TurnOffAsync(string entityId);
    Task<bool> ToggleAsync(string entityId);
    Task<bool> SetBrightnessAsync(string entityId, int brightness);
    Task<bool> SetColorAsync(string entityId, int red, int green, int blue);
    Task SubscribeToEntityAsync(string entityId);
    Task UnsubscribeFromEntityAsync(string entityId);
}

public class EntityStateChangedEventArgs : EventArgs
{
    public string EntityId { get; set; } = "";
    public string State { get; set; } = "";
    public Dictionary<string, object> Attributes { get; set; } = new();
}
