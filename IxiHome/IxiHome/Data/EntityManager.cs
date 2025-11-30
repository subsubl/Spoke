using IXICore;
using IXICore.Meta;
using IxiHome.Interfaces;
using IxiHome.Network;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace IxiHome.Data;

/// <summary>
/// Manages all entities in the app
/// </summary>
// TODO: Fully implement IEntityManager interface (signature mismatches to resolve)
public class EntityManager // : IEntityManager
{
    private static EntityManager? _instance;
    public static EntityManager Instance => _instance ??= new EntityManager();

    public ObservableCollection<Entity> Entities { get; private set; } = new();

    private readonly string _entitiesFilePath;

    private EntityManager()
    {
        _entitiesFilePath = Meta.Config.entitiesFilePath;
        
        // Subscribe to websocket events for real-time updates
        WebSocketManager.Instance.EntityStateChanged += OnEntityStateChanged;
        WebSocketManager.Instance.NotificationReceived += OnNotificationReceived;
    }

    /// <summary>
    /// Load entities from local storage
    /// </summary>
    public async Task LoadEntitiesAsync()
    {
        try
        {
            if (!File.Exists(_entitiesFilePath))
            {
                Logging.info("No entities file found, starting with empty collection");
                return;
            }

            var json = await File.ReadAllTextAsync(_entitiesFilePath);
            var savedEntities = JsonConvert.DeserializeObject<List<EntityDto>>(json);

            if (savedEntities == null)
            {
                Logging.warn("Failed to deserialize entities file");
                return;
            }

            Entities.Clear();
            foreach (var dto in savedEntities)
            {
                var entity = CreateEntityFromDto(dto);
                if (entity != null)
                {
                    Entities.Add(entity);
                }
            }

            Logging.info($"Loaded {Entities.Count} entities from storage");
        }
        catch (Exception ex)
        {
            Logging.error($"Error loading entities: {ex.Message}");
        }
    }

    /// <summary>
    /// Save entities to local storage
    /// </summary>
    public async Task SaveEntitiesAsync()
    {
        try
        {
            var dtos = Entities.Select(e => CreateDtoFromEntity(e)).ToList();
            var json = JsonConvert.SerializeObject(dtos, Formatting.Indented);
            
            await File.WriteAllTextAsync(_entitiesFilePath, json);
            Logging.info($"Saved {Entities.Count} entities to storage");
        }
        catch (Exception ex)
        {
            Logging.error($"Error saving entities: {ex.Message}");
        }
    }

    /// <summary>
    /// Add a new entity
    /// </summary>
    public async Task<Entity> AddEntityAsync(Entity entity)
    {
        entity.Id = Guid.NewGuid().ToString();
        entity.Order = Entities.Count;
        Entities.Add(entity);
        
        await SaveEntitiesAsync();
        Logging.info($"Added entity: {entity.Name} ({entity.EntityId})");
        
        return entity;
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    public async Task UpdateEntityAsync(Entity entity)
    {
        var existing = Entities.FirstOrDefault(e => e.Id == entity.Id);
        if (existing != null)
        {
            var index = Entities.IndexOf(existing);
            Entities[index] = entity;
            await SaveEntitiesAsync();
            Logging.info($"Updated entity: {entity.Name} ({entity.EntityId})");
        }
    }

    /// <summary>
    /// Remove an entity
    /// </summary>
    public async Task RemoveEntityAsync(Entity entity)
    {
        Entities.Remove(entity);
        await SaveEntitiesAsync();
        Logging.info($"Removed entity: {entity.Name} ({entity.EntityId})");
    }

    /// <summary>
    /// Reorder entities
    /// </summary>
    public async Task ReorderEntitiesAsync(Entity entity, int newOrder)
    {
        var oldOrder = entity.Order;
        entity.Order = newOrder;

        // Adjust order of other entities
        foreach (var e in Entities.Where(e => e.Id != entity.Id))
        {
            if (newOrder < oldOrder && e.Order >= newOrder && e.Order < oldOrder)
            {
                e.Order++;
            }
            else if (newOrder > oldOrder && e.Order > oldOrder && e.Order <= newOrder)
            {
                e.Order--;
            }
        }

        // Re-sort collection
        var sorted = Entities.OrderBy(e => e.Order).ToList();
        Entities.Clear();
        foreach (var e in sorted)
        {
            Entities.Add(e);
        }

        await SaveEntitiesAsync();
    }

    /// <summary>
    /// Get entity by ID
    /// </summary>
    public Entity? GetEntityById(string id)
    {
        return Entities.FirstOrDefault(e => e.Id == id);
    }

    /// <summary>
    /// Get entity by Home Assistant entity_id
    /// </summary>
    public Entity? GetEntityByEntityId(string entityId)
    {
        return Entities.FirstOrDefault(e => e.EntityId == entityId);
    }

    /// <summary>
    /// Update entity state from Home Assistant
    /// </summary>
    public void UpdateEntityState(string entityId, string state, Dictionary<string, object> attributes)
    {
        var entity = GetEntityByEntityId(entityId);
        if (entity != null)
        {
            entity.UpdateState(state, attributes);
        }
    }

    /// <summary>
    /// Create entity from DTO
    /// </summary>
    private Entity? CreateEntityFromDto(EntityDto dto)
    {
        Entity? entity = dto.DisplayType switch
        {
            "toggle" => new ToggleEntity(),
            "sensor" => new SensorEntity(),
            "light" => new LightEntity(),
            "gauge" => new GaugeEntity(),
            "graph" => new GraphEntity(),
            _ => null
        };

        if (entity == null)
        {
            Logging.warn($"Unknown display type: {dto.DisplayType}");
            return null;
        }

        entity.Id = dto.Id;
        entity.EntityId = dto.EntityId;
        entity.Name = dto.Name;
        entity.Domain = dto.Domain;
        entity.State = dto.State;
        entity.Icon = dto.Icon;
        entity.DisplayType = dto.DisplayType;
        entity.Order = dto.Order;
        entity.Attributes = dto.Attributes;
        entity.Config = dto.Config;

        return entity;
    }

    /// <summary>
    /// Create DTO from entity
    /// </summary>
    private EntityDto CreateDtoFromEntity(Entity entity)
    {
        return new EntityDto
        {
            Id = entity.Id,
            EntityId = entity.EntityId,
            Name = entity.Name,
            Domain = entity.Domain,
            State = entity.State,
            Icon = entity.Icon,
            DisplayType = entity.DisplayType,
            Order = entity.Order,
            Attributes = entity.Attributes,
            Config = entity.Config
        };
    }

    /// <summary>
    /// Handle entity state changes from websocket
    /// </summary>
    private void OnEntityStateChanged(object? sender, Network.EntityStateChangedEventArgs e)
    {
        // Entity is already updated by WebSocketManager, just log the change
        Logging.info($"Entity state updated via websocket: {e.Entity.Name} ({e.Entity.EntityId}) = {e.Entity.State}");
    }

    /// <summary>
    /// Handle notifications from websocket
    /// </summary>
    private void OnNotificationReceived(object? sender, NotificationReceivedEventArgs e)
    {
        Logging.info($"Notification received: {e.Title} - {e.Body}");
        Notifications.NotificationManager.Instance.ShowSystemNotification(e.Title, e.Body);
    }
}

/// <summary>
/// Data transfer object for entity serialization
/// </summary>
internal class EntityDto
{
    public string Id { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Domain { get; set; } = "";
    public string State { get; set; } = "";
    public string Icon { get; set; } = "";
    public string DisplayType { get; set; } = "";
    public int Order { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
    public Dictionary<string, string> Config { get; set; } = new();
}
