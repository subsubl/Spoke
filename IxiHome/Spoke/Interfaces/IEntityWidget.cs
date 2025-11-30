using IxiHome.Data;

namespace Spoke.Interfaces;

public interface IEntityWidget
{
    Entity? Entity { get; set; }
    
    Task OnStateChangedAsync(string newState, Dictionary<string, object> attributes);
    void UpdateUI();
    Task OnInteractionAsync();
}


