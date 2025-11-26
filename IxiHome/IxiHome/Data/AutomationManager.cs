using IXICore;
using IXICore.Meta;
using IxiHome.Network;

namespace IxiHome.Data;

/// <summary>
/// Manages Home Assistant automations
/// </summary>
public class AutomationManager
{
    private static AutomationManager? _instance;
    public static AutomationManager Instance => _instance ??= new AutomationManager();

    private readonly Dictionary<string, Automation> _automations = new();
    private readonly object _automationsLock = new();

    private CancellationTokenSource? _automationProcessorCts;
    private Task? _automationProcessorTask;

    private AutomationManager() { }

    /// <summary>
    /// Start the automation processing
    /// </summary>
    public void Start()
    {
        if (_automationProcessorTask != null)
            return;

        _automationProcessorCts = new CancellationTokenSource();
        _automationProcessorTask = Task.Run(() => ProcessAutomationsAsync(_automationProcessorCts.Token));

        Logging.info("AutomationManager started");
    }

    /// <summary>
    /// Stop the automation processing
    /// </summary>
    public void Stop()
    {
        _automationProcessorCts?.Cancel();
        _automationProcessorTask?.Wait();
        _automationProcessorTask = null;
        _automationProcessorCts = null;

        Logging.info("AutomationManager stopped");
    }

    /// <summary>
    /// Get all automations
    /// </summary>
    public IEnumerable<Automation> GetAutomations()
    {
        lock (_automationsLock)
        {
            return _automations.Values.ToList();
        }
    }

    /// <summary>
    /// Get an automation by ID
    /// </summary>
    public Automation? GetAutomation(string automationId)
    {
        lock (_automationsLock)
        {
            return _automations.TryGetValue(automationId, out var automation) ? automation : null;
        }
    }

    /// <summary>
    /// Add or update an automation
    /// </summary>
    public void AddOrUpdateAutomation(Automation automation)
    {
        lock (_automationsLock)
        {
            _automations[automation.Id] = automation;
        }
    }

    /// <summary>
    /// Remove an automation
    /// </summary>
    public void RemoveAutomation(string automationId)
    {
        lock (_automationsLock)
        {
            _automations.Remove(automationId);
        }
    }

    /// <summary>
    /// Trigger automations based on an event
    /// </summary>
    public async Task TriggerAutomationsAsync(AutomationTriggerEvent triggerEvent)
    {
        var automationsToTrigger = new List<Automation>();

        lock (_automationsLock)
        {
            foreach (var automation in _automations.Values)
            {
                if (automation.ShouldTrigger(triggerEvent))
                {
                    automationsToTrigger.Add(automation);
                }
            }
        }

        foreach (var automation in automationsToTrigger)
        {
            try
            {
                Logging.info($"Triggering automation: {automation.Name}");
                await automation.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Logging.error($"Failed to execute automation {automation.Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Process time-based automations
    /// </summary>
    private async Task ProcessAutomationsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Check time-based triggers every minute
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;

                var timeEvent = new AutomationTriggerEvent
                {
                    Type = "time"
                };

                await TriggerAutomationsAsync(timeEvent);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logging.error($"Error in automation processor: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Create a simple automation
    /// </summary>
    public Automation CreateSimpleAutomation(string name, string triggerEntityId, string triggerState,
                                           string actionEntityId, string actionState)
    {
        var automation = new Automation
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = $"When {triggerEntityId} turns {triggerState}, set {actionEntityId} to {actionState}"
        };

        // Add trigger
        automation.AddTrigger(new EntityStateTrigger
        {
            EntityId = triggerEntityId,
            ToState = triggerState
        });

        // Add action
        automation.AddAction(new EntityStateAction
        {
            EntityId = actionEntityId,
            State = actionState
        });

        AddOrUpdateAutomation(automation);
        return automation;
    }

    /// <summary>
    /// Load automations from storage
    /// </summary>
    public void LoadAutomations()
    {
        // TODO: Implement persistent storage
        Logging.info("AutomationManager: LoadAutomations not implemented yet");
    }

    /// <summary>
    /// Save automations to storage
    /// </summary>
    public void SaveAutomations()
    {
        // TODO: Implement persistent storage
        Logging.info("AutomationManager: SaveAutomations not implemented yet");
    }
}