using IXICore;
using IXICore.Meta;
using IxiHome.Network;

namespace Spoke.Data;

/// <summary>
/// Manages Home Assistant scenes
/// </summary>
public class SceneManager
{
    private static SceneManager? _instance;
    public static SceneManager Instance => _instance ??= new SceneManager();

    private readonly Dictionary<string, Scene> _scenes = new();
    private readonly object _scenesLock = new();

    private SceneManager() { }

    /// <summary>
    /// Get all scenes
    /// </summary>
    public IEnumerable<Scene> GetScenes()
    {
        lock (_scenesLock)
        {
            return _scenes.Values.ToList();
        }
    }

    /// <summary>
    /// Get a scene by ID
    /// </summary>
    public Scene? GetScene(string sceneId)
    {
        lock (_scenesLock)
        {
            return _scenes.TryGetValue(sceneId, out var scene) ? scene : null;
        }
    }

    /// <summary>
    /// Add or update a scene
    /// </summary>
    public void AddOrUpdateScene(Scene scene)
    {
        lock (_scenesLock)
        {
            _scenes[scene.Id] = scene;
        }
    }

    /// <summary>
    /// Remove a scene
    /// </summary>
    public void RemoveScene(string sceneId)
    {
        lock (_scenesLock)
        {
            _scenes.Remove(sceneId);
        }
    }

    /// <summary>
    /// Activate a scene by setting all its entity states
    /// </summary>
    public async Task ActivateSceneAsync(string sceneId)
    {
        var scene = GetScene(sceneId);
        if (scene == null)
        {
            Logging.warn($"Scene {sceneId} not found");
            return;
        }

        Logging.info($"Activating scene: {scene.Name}");

        foreach (var entityState in scene.Entities)
        {
            try
            {
                string command = entityState.Value.State.ToLower() switch
                {
                    "on" => "turn_on",
                    "off" => "turn_off",
                    _ => "set_state" // fallback
                };

                if (Meta.Node.Instance.quixiClient != null)
                {
                    await Meta.Node.Instance.quixiClient.SendCommandAsync(
                        entityState.Key,
                        command,
                        entityState.Value.Attributes);
                }
            }
            catch (Exception ex)
            {
                Logging.error($"Failed to set entity {entityState.Key} in scene {scene.Name}: {ex.Message}");
            }
        }

        scene.LastActivated = DateTime.Now;
        scene.IsActive = true;

        // Notify Home Assistant if connected
        if (Meta.Node.Instance.quixiClient != null)
        {
            try
            {
                var data = new Dictionary<string, object>
                {
                    ["scene_id"] = sceneId
                };
                await Meta.Node.Instance.quixiClient.SendCommandAsync("scene", "activate", data);
            }
            catch (Exception ex)
            {
                Logging.error($"Failed to notify Home Assistant of scene activation: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Create a scene from current entity states
    /// </summary>
    public Scene CreateSceneFromCurrentStates(string name, IEnumerable<string> entityIds)
    {
        var scene = new Scene
        {
            Id = Guid.NewGuid().ToString(),
            Name = name
        };

        foreach (var entityId in entityIds)
        {
            var entity = EntityManager.Instance.GetEntityByEntityId(entityId);
            if (entity != null)
            {
                scene.AddEntityState(entityId, entity.State, entity.Attributes);
            }
        }

        AddOrUpdateScene(scene);
        return scene;
    }

    /// <summary>
    /// Load scenes from storage
    /// </summary>
    public void LoadScenes()
    {
        // TODO: Implement persistent storage
        Logging.info("SceneManager: LoadScenes not implemented yet");
    }

    /// <summary>
    /// Save scenes to storage
    /// </summary>
    public void SaveScenes()
    {
        // TODO: Implement persistent storage
        Logging.info("SceneManager: SaveScenes not implemented yet");
    }
}

