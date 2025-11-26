using IxiHome.Data;
using System.Collections.ObjectModel;

namespace IxiHome.Interfaces;

public interface IEntityManager
{
    ObservableCollection<Entity> Entities { get; }
    
    Task LoadEntitiesAsync();
    Task SaveEntitiesAsync();
    Task AddEntityAsync(Entity entity);
    Task UpdateEntityAsync(Entity entity);
    Task RemoveEntityAsync(string entityId);
    Entity? GetEntityById(string entityId);
    Task ReorderEntitiesAsync(int oldIndex, int newIndex);
}
