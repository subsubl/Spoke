using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace Spoke.Data;

/// <summary>
/// Represents a Home Assistant automation rule
/// </summary>
public partial class Automation : ObservableObject
{
    [ObservableProperty]
    private string _id = "";

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private DateTime _lastTriggered = DateTime.MinValue;

    [ObservableProperty]
    private int _triggerCount = 0;

    /// <summary>
    /// Triggers that can activate this automation
    /// </summary>
    [JsonProperty("triggers")]
    public List<AutomationTrigger> Triggers { get; set; } = new();

    /// <summary>
    /// Optional conditions that must be met for the automation to run
    /// </summary>
    [JsonProperty("conditions")]
    public List<AutomationCondition> Conditions { get; set; } = new();

    /// <summary>
    /// Actions to perform when triggered and conditions are met
    /// </summary>
    [JsonProperty("actions")]
    public List<AutomationAction> Actions { get; set; } = new();

    /// <summary>
    /// Add a trigger to the automation
    /// </summary>
    public void AddTrigger(AutomationTrigger trigger)
    {
        Triggers.Add(trigger);
    }

    /// <summary>
    /// Add a condition to the automation
    /// </summary>
    public void AddCondition(AutomationCondition condition)
    {
        Conditions.Add(condition);
    }

    /// <summary>
    /// Add an action to the automation
    /// </summary>
    public void AddAction(AutomationAction action)
    {
        Actions.Add(action);
    }

    /// <summary>
    /// Check if this automation should trigger based on the event
    /// </summary>
    public bool ShouldTrigger(AutomationTriggerEvent triggerEvent)
    {
        if (!IsEnabled)
            return false;

        // Check if any trigger matches
        bool hasMatchingTrigger = Triggers.Any(t => t.Matches(triggerEvent));
        if (!hasMatchingTrigger)
            return false;

        // Check if all conditions are met
        bool conditionsMet = Conditions.All(c => c.IsMet());
        return conditionsMet;
    }

    /// <summary>
    /// Execute the automation actions
    /// </summary>
    public async Task ExecuteAsync()
    {
        LastTriggered = DateTime.Now;
        TriggerCount++;

        foreach (var action in Actions)
        {
            try
            {
                await action.ExecuteAsync();
            }
            catch (Exception ex)
            {
                // Log error but continue with other actions
                IXICore.Meta.Logging.error($"Failed to execute automation action: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// Base class for automation triggers
/// </summary>
public abstract class AutomationTrigger
{
    [JsonProperty("type")]
    public string Type { get; set; } = "";

    /// <summary>
    /// Check if this trigger matches the given event
    /// </summary>
    public abstract bool Matches(AutomationTriggerEvent triggerEvent);
}

/// <summary>
/// Trigger for entity state changes
/// </summary>
public class EntityStateTrigger : AutomationTrigger
{
    public EntityStateTrigger()
    {
        Type = "state";
    }

    [JsonProperty("entity_id")]
    public string EntityId { get; set; } = "";

    [JsonProperty("from_state")]
    public string? FromState { get; set; }

    [JsonProperty("to_state")]
    public string? ToState { get; set; }

    public override bool Matches(AutomationTriggerEvent triggerEvent)
    {
        if (triggerEvent.Type != "state_changed")
            return false;

        if (triggerEvent.EntityId != EntityId)
            return false;

        if (FromState != null && triggerEvent.OldState != FromState)
            return false;

        if (ToState != null && triggerEvent.NewState != ToState)
            return false;

        return true;
    }
}

/// <summary>
/// Trigger for time-based events
/// </summary>
public class TimeTrigger : AutomationTrigger
{
    public TimeTrigger()
    {
        Type = "time";
    }

    [JsonProperty("at")]
    public TimeSpan At { get; set; }

    public override bool Matches(AutomationTriggerEvent triggerEvent)
    {
        if (triggerEvent.Type != "time")
            return false;

        // Check if current time matches (within 1 minute tolerance)
        var currentTime = DateTime.Now.TimeOfDay;
        return Math.Abs((currentTime - At).TotalMinutes) < 1;
    }
}

/// <summary>
/// Base class for automation conditions
/// </summary>
public abstract class AutomationCondition
{
    [JsonProperty("type")]
    public string Type { get; set; } = "";

    /// <summary>
    /// Check if this condition is currently met
    /// </summary>
    public abstract bool IsMet();
}

/// <summary>
/// Condition based on entity state
/// </summary>
public class EntityStateCondition : AutomationCondition
{
    public EntityStateCondition()
    {
        Type = "state";
    }

    [JsonProperty("entity_id")]
    public string EntityId { get; set; } = "";

    [JsonProperty("state")]
    public string State { get; set; } = "";

    public override bool IsMet()
    {
        var entity = EntityManager.Instance.GetEntityByEntityId(EntityId);
        return entity?.State == State;
    }
}

/// <summary>
/// Base class for automation actions
/// </summary>
public abstract class AutomationAction
{
    [JsonProperty("type")]
    public string Type { get; set; } = "";

    /// <summary>
    /// Execute this action
    /// </summary>
    public abstract Task ExecuteAsync();
}

/// <summary>
/// Action to set an entity state
/// </summary>
public class EntityStateAction : AutomationAction
{
    public EntityStateAction()
    {
        Type = "state";
    }

    [JsonProperty("entity_id")]
    public string EntityId { get; set; } = "";

    [JsonProperty("state")]
    public string State { get; set; } = "";

    [JsonProperty("attributes")]
    public Dictionary<string, object> Attributes { get; set; } = new();

    public override async Task ExecuteAsync()
    {
        if (Meta.Node.Instance.quixiClient != null)
        {
            string command = State.ToLower() switch
            {
                "on" => "turn_on",
                "off" => "turn_off",
                _ => "set_state" // fallback
            };

            await Meta.Node.Instance.quixiClient.SendCommandAsync(EntityId, command, Attributes);
        }
    }
}

/// <summary>
/// Action to activate a scene
/// </summary>
public class SceneAction : AutomationAction
{
    public SceneAction()
    {
        Type = "scene";
    }

    [JsonProperty("scene_id")]
    public string SceneId { get; set; } = "";

    public override async Task ExecuteAsync()
    {
        await SceneManager.Instance.ActivateSceneAsync(SceneId);
    }
}

/// <summary>
/// Event that can trigger automations
/// </summary>
public class AutomationTriggerEvent
{
    public string Type { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string? OldState { get; set; }
    public string? NewState { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
}

