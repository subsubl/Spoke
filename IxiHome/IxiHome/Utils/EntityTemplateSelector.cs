using IxiHome.Data;

namespace IxiHome.Utils;

/// <summary>
/// Selects the appropriate DataTemplate based on entity display type
/// </summary>
public class EntityTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ToggleTemplate { get; set; }
    public DataTemplate? SensorTemplate { get; set; }
    public DataTemplate? LightTemplate { get; set; }
    public DataTemplate? GaugeTemplate { get; set; }
    public DataTemplate? GraphTemplate { get; set; }
    public DataTemplate? DefaultTemplate { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        if (item is not Entity entity)
            return DefaultTemplate;

        return entity.DisplayType.ToLower() switch
        {
            "toggle" => ToggleTemplate,
            "sensor" => SensorTemplate,
            "light" => LightTemplate ?? ToggleTemplate, // Lights use toggle template
            "gauge" => GaugeTemplate,
            "graph" => GraphTemplate,
            _ => DefaultTemplate ?? SensorTemplate
        };
    }
}
