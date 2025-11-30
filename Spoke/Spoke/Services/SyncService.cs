using IXICore;
using IXICore.Meta;
using IxiHome.Data;
using IxiHome.Network;
using IxiHome.Interfaces;

namespace Spoke.Services;

/// <summary>
/// Service for synchronizing entity states with Home Assistant via QuIXI
/// </summary>
public class SyncService
{
    private static SyncService? _instance;
    public static SyncService Instance => _instance ??= new SyncService();
    
    private bool _isRunning;
    private CancellationTokenSource? _cancellationTokenSource;
    
    private SyncService() { }
    
    /// <summary>
    /// Start the sync service
    /// </summary>
    public async Task StartAsync()
    {
        if (_isRunning)
        {
            Logging.warn("SyncService already running");
            return;
        }
        
        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        Logging.info("SyncService started");
        
        // Subscribe to entity state changes from QuIXI
        if (Meta.Node.Instance.quixiClient != null)
        {
            Meta.Node.Instance.quixiClient.EntityStateChanged += OnEntityStateChanged;
        }
        
        // Start polling for updates (fallback if WebSocket not available)
        _ = Task.Run(() => PollEntitiesAsync(_cancellationTokenSource.Token));
    }
    
    /// <summary>
    /// Stop the sync service
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            return;
        }
        
        _isRunning = false;
        _cancellationTokenSource?.Cancel();
        
        if (Meta.Node.Instance.quixiClient != null)
        {
            Meta.Node.Instance.quixiClient.EntityStateChanged -= OnEntityStateChanged;
            await Meta.Node.Instance.quixiClient.DisconnectWebSocketAsync();
        }
        
        Logging.info("SyncService stopped");
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Poll entities for state updates
    /// </summary>
    private async Task PollEntitiesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                
                // Skip polling if WebSocket is connected for real-time updates
                if (Meta.Node.Instance.quixiClient?.IsWebSocketConnected == true)
                {
                    continue;
                }
                
                if (Meta.Node.Instance.quixiClient == null)
                {
                    continue;
                }
                
                // Fetch latest states for all entities
                var remoteEntities = await Meta.Node.Instance.quixiClient.GetEntitiesAsync();
                
                foreach (var entity in EntityManager.Instance.Entities)
                {
                    var remoteEntity = remoteEntities.FirstOrDefault(e => e.EntityId == entity.EntityId);
                    if (remoteEntity != null)
                    {
                        await UpdateEntityFromHomeAssistantAsync(entity, remoteEntity);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logging.error($"Error polling entities: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Handle entity state change event from QuIXI
    /// </summary>
    private async void OnEntityStateChanged(object? sender, Interfaces.EntityStateChangedEventArgs e)
    {
        try
        {
            var entity = EntityManager.Instance.GetEntityById(e.EntityId);
            if (entity == null)
            {
                return;
            }
            
            // Update entity state
            entity.State = e.State;
            entity.LastUpdated = DateTime.UtcNow;
            
            // Update specific entity type properties
            if (entity is ToggleEntity toggleEntity)
            {
                toggleEntity.IsOn = e.State.Equals("on", StringComparison.OrdinalIgnoreCase);
            }
            else if (entity is SensorEntity sensorEntity)
            {
                // Try to parse state as numeric value, fallback to raw state string
                if (double.TryParse(e.State, out double numericValue))
                {
                    sensorEntity.Value = numericValue;
                }
                else
                {
                    sensorEntity.Value = 0; // Default for non-numeric states
                }
            }
            else if (entity is LightEntity lightEntity)
            {
                lightEntity.IsOn = e.State.Equals("on", StringComparison.OrdinalIgnoreCase);
                if (e.Attributes.ContainsKey("brightness"))
                {
                    lightEntity.Brightness = Convert.ToInt32(e.Attributes["brightness"]);
                }
            }
            
            await EntityManager.Instance.UpdateEntityAsync(entity);
            
            // Trigger automations based on state change
            var triggerEvent = new AutomationTriggerEvent
            {
                Type = "state_changed",
                EntityId = e.EntityId,
                OldState = entity.State, // This might not be accurate, but we don't have old state here
                NewState = e.State,
                Attributes = e.Attributes ?? new Dictionary<string, object>()
            };
            await AutomationManager.Instance.TriggerAutomationsAsync(triggerEvent);
            
            // Show notification for state changes
            Notifications.NotificationManager.Instance.ShowEntityStateChangedNotification(
                entity.Id, e.State, entity.Name);
        }
        catch (Exception ex)
        {
            Logging.error($"Error handling entity state change: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Update local entity from Home Assistant state
    /// </summary>
    private async Task UpdateEntityFromHomeAssistantAsync(Entity localEntity, HomeAssistantEntity haEntity)
    {
        bool hasChanges = false;
        
        if (localEntity.State != haEntity.State)
        {
            localEntity.State = haEntity.State;
            hasChanges = true;
        }
        
        // Update type-specific properties
        if (localEntity is ToggleEntity toggleEntity)
        {
            bool isOn = haEntity.State.Equals("on", StringComparison.OrdinalIgnoreCase);
            if (toggleEntity.IsOn != isOn)
            {
                toggleEntity.IsOn = isOn;
                hasChanges = true;
            }
        }
        else if (localEntity is SensorEntity sensorEntity)
        {
            if (double.TryParse(haEntity.State, out double value))
            {
                if (sensorEntity.Value != value)
                {
                    sensorEntity.Value = value;
                    hasChanges = true;
                }
            }
        }
        else if (localEntity is LightEntity lightEntity)
        {
            bool isOn = haEntity.State.Equals("on", StringComparison.OrdinalIgnoreCase);
            if (lightEntity.IsOn != isOn)
            {
                lightEntity.IsOn = isOn;
                hasChanges = true;
            }
            
            if (haEntity.Attributes.ContainsKey("brightness"))
            {
                if (int.TryParse(haEntity.Attributes["brightness"]?.ToString(), out int brightness))
                {
                    if (lightEntity.Brightness != brightness)
                    {
                        lightEntity.Brightness = brightness;
                        hasChanges = true;
                    }
                }
            }
        }
        
        if (hasChanges)
        {
            localEntity.LastUpdated = DateTime.UtcNow;
            await EntityManager.Instance.UpdateEntityAsync(localEntity);
        }
    }
}


