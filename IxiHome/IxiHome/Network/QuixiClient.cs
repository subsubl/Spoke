using IXICore;
using IXICore.Meta;
using IxiHome.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace IxiHome.Network;

/// <summary>
/// Client for communicating with QuIXI bridge
/// </summary>
// TODO: Fully implement IQuixiClient interface (signature mismatches to resolve)
public class QuixiClient // : IQuixiClient
{
    private readonly string _host;
    private readonly int _port;
    private readonly bool _secure;
    private readonly string _username;
    private readonly string _password;
    private readonly HttpClient _httpClient;
    private string _baseUrl;

    public bool IsConnected { get; private set; }

    public event EventHandler<EntityStateChangedEventArgs>? EntityStateChanged;
    public event EventHandler<string>? ConnectionStatusChanged;

    public QuixiClient(string host, int port, bool secure = false, string username = "", string password = "")
    {
        _host = host;
        _port = port;
        _secure = secure;
        _username = username;
        _password = password;

        string protocol = _secure ? "https" : "http";
        _baseUrl = $"{protocol}://{_host}:{_port}";

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Set up basic authentication if credentials provided
        if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
        {
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        }

        Logging.info($"QuixiClient created for {_baseUrl}");
    }

    /// <summary>
    /// Test connection to QuIXI
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/status");
            IsConnected = response.IsSuccessStatusCode;
            
            ConnectionStatusChanged?.Invoke(this, IsConnected ? "Connected" : "Failed");
            
            return IsConnected;
        }
        catch (Exception ex)
        {
            Logging.error($"QuixiClient connection test failed: {ex.Message}");
            IsConnected = false;
            ConnectionStatusChanged?.Invoke(this, $"Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get all available Home Assistant entities via QuIXI
    /// </summary>
    public async Task<List<HomeAssistantEntity>> GetEntitiesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/homeassistant/entities");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var entities = JsonConvert.DeserializeObject<List<HomeAssistantEntity>>(content);
                return entities ?? new List<HomeAssistantEntity>();
            }
            else
            {
                Logging.error($"Failed to get entities: {response.StatusCode}");
                return new List<HomeAssistantEntity>();
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Error getting entities: {ex.Message}");
            return new List<HomeAssistantEntity>();
        }
    }

    /// <summary>
    /// Get state of a specific entity
    /// </summary>
    public async Task<EntityState?> GetEntityStateAsync(string entityId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/homeassistant/state/{entityId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<EntityState>(content);
            }
            else
            {
                Logging.error($"Failed to get entity state for {entityId}: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Error getting entity state for {entityId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Send command to toggle/control an entity
    /// </summary>
    public async Task<bool> SendCommandAsync(string entityId, string command, Dictionary<string, object>? parameters = null)
    {
        try
        {
            var payload = new
            {
                entity_id = entityId,
                command = command,
                parameters = parameters ?? new Dictionary<string, object>()
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/homeassistant/command", content);
            
            if (response.IsSuccessStatusCode)
            {
                Logging.info($"Command '{command}' sent successfully to {entityId}");
                return true;
            }
            else
            {
                Logging.error($"Failed to send command to {entityId}: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Error sending command to {entityId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Turn on an entity (light, switch, etc.)
    /// </summary>
    public Task<bool> TurnOnAsync(string entityId, Dictionary<string, object>? parameters = null)
    {
        return SendCommandAsync(entityId, "turn_on", parameters);
    }

    /// <summary>
    /// Turn off an entity
    /// </summary>
    public Task<bool> TurnOffAsync(string entityId)
    {
        return SendCommandAsync(entityId, "turn_off");
    }

    /// <summary>
    /// Toggle an entity state
    /// </summary>
    public Task<bool> ToggleAsync(string entityId)
    {
        return SendCommandAsync(entityId, "toggle");
    }

    /// <summary>
    /// Subscribe to entity state updates (placeholder for WebSocket/MQTT implementation)
    /// </summary>
    public void SubscribeToUpdates(List<string> entityIds)
    {
        // TODO: Implement WebSocket or MQTT subscription for real-time updates
        // For now, this is a placeholder that will poll for updates
        Logging.info($"Subscribing to updates for {entityIds.Count} entities");
        
        Task.Run(async () =>
        {
            while (IsConnected)
            {
                foreach (var entityId in entityIds)
                {
                    var state = await GetEntityStateAsync(entityId);
                    if (state != null)
                    {
                        EntityStateChanged?.Invoke(this, new EntityStateChangedEventArgs
                        {
                            EntityId = entityId,
                            State = state.State,
                            Attributes = state.Attributes
                        });
                    }
                }
                
                await Task.Delay(Meta.Config.entityRefreshInterval);
            }
        });
    }

    /// <summary>
    /// Disconnect from QuIXI
    /// </summary>
    public void Disconnect()
    {
        IsConnected = false;
        ConnectionStatusChanged?.Invoke(this, "Disconnected");
        Logging.info("QuixiClient disconnected");
    }
    
    /// <summary>
    /// Subscribe to entity state changes
    /// </summary>
    public async Task SubscribeToEntityAsync(string entityId)
    {
        // TODO: Implement WebSocket/MQTT subscription for real-time updates
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Unsubscribe from entity state changes
    /// </summary>
    public async Task UnsubscribeFromEntityAsync(string entityId)
    {
        // TODO: Implement WebSocket/MQTT unsubscription
        await Task.CompletedTask;
    }
}

/// <summary>
/// Represents a Home Assistant entity
/// </summary>
public class HomeAssistantEntity
{
    [JsonProperty("entity_id")]
    public string EntityId { get; set; } = "";

    [JsonProperty("domain")]
    public string Domain { get; set; } = "";

    [JsonProperty("friendly_name")]
    public string FriendlyName { get; set; } = "";

    [JsonProperty("state")]
    public string State { get; set; } = "";

    [JsonProperty("attributes")]
    public Dictionary<string, object> Attributes { get; set; } = new();
}

/// <summary>
/// Represents the state of an entity
/// </summary>
public class EntityState
{
    [JsonProperty("entity_id")]
    public string EntityId { get; set; } = "";

    [JsonProperty("state")]
    public string State { get; set; } = "";

    [JsonProperty("attributes")]
    public Dictionary<string, object> Attributes { get; set; } = new();

    [JsonProperty("last_changed")]
    public DateTime LastChanged { get; set; }

    [JsonProperty("last_updated")]
    public DateTime LastUpdated { get; set; }
}
