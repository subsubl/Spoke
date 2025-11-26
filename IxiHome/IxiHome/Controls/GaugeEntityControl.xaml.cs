using IxiHome.Data;

namespace IxiHome.Controls;

public partial class GaugeEntityControl : ContentView
{
    public static readonly BindableProperty EntityProperty =
        BindableProperty.Create(nameof(Entity), typeof(GaugeEntity), typeof(GaugeEntityControl), null, propertyChanged: OnEntityChanged);

    public GaugeEntity? Entity
    {
        get => (GaugeEntity?)GetValue(EntityProperty);
        set => SetValue(EntityProperty, value);
    }

    public GaugeEntityControl()
    {
        InitializeComponent();
    }

    private static void OnEntityChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (GaugeEntityControl)bindable;
        control.UpdateGauge();
    }

    private void UpdateGauge()
    {
        if (Entity == null) return;

        // Update value display
        ValueLabel.Text = Entity.Value.ToString("F1");

        // Calculate percentage for visual representation
        var percentage = Entity.GetPercentage();
        
        // Note: Full circular progress would require custom drawing using Microsoft.Maui.Graphics
        // For now, we use opacity as a simple visual indicator
        GaugeProgress.Opacity = Math.Min(1.0, percentage / 100.0);
        
        // Color coding based on percentage
        if (percentage >= 80)
        {
            GaugeProgress.Stroke = new SolidColorBrush(Colors.Red);
        }
        else if (percentage >= 60)
        {
            GaugeProgress.Stroke = new SolidColorBrush(Colors.Orange);
        }
        else if (percentage >= 40)
        {
            GaugeProgress.Stroke = new SolidColorBrush(Colors.Yellow);
        }
        else
        {
            GaugeProgress.Stroke = new SolidColorBrush(Colors.Green);
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        UpdateGauge();
    }

    private async void OnTapped(object sender, EventArgs e)
    {
        if (Entity == null) return;
        
        // Navigate to entity details
        await Shell.Current.GoToAsync($"entitydetail?entityId={Entity.Id}");
    }
}
