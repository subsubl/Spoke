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
    public DataTemplate? ClimateTemplate { get; set; }
    public DataTemplate? GaugeTemplate { get; set; }
    public DataTemplate? GraphTemplate { get; set; }
    public DataTemplate? DefaultTemplate { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        if (item is not Entity entity)
            return DefaultTemplate;

        // First check by type
        if (entity is LightEntity)
            return LightTemplate ?? ToggleTemplate;
        if (entity is ClimateEntity)
            return ClimateTemplate ?? SensorTemplate;
        if (entity is ToggleEntity)
            return ToggleTemplate;
        if (entity is GaugeEntity)
            return GaugeTemplate;
        if (entity is GraphEntity)
            return GraphTemplate;
        if (entity is SensorEntity)
            return SensorTemplate;

        // Fallback to display type
        return entity.DisplayType.ToLower() switch
        {
            "toggle" => ToggleTemplate,
            "sensor" => SensorTemplate,
            "light" => LightTemplate ?? ToggleTemplate,
            "gauge" => GaugeTemplate,
            "graph" => GraphTemplate,
            _ => DefaultTemplate ?? SensorTemplate
        };
    }
}
