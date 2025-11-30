using IxiHome.Data;

namespace Spoke.Controls;

public partial class SensorEntityControl : ContentView
{
    public static readonly BindableProperty EntityProperty =
        BindableProperty.Create(nameof(Entity), typeof(SensorEntity), typeof(SensorEntityControl), null);

    public SensorEntity? Entity
    {
        get => (SensorEntity?)GetValue(EntityProperty);
        set => SetValue(EntityProperty, value);
    }

    public SensorEntityControl()
    {
        InitializeComponent();
    }

    private async void OnTapped(object sender, EventArgs e)
    {
        if (Entity == null) return;
        
        // Navigate to entity details
        await Shell.Current.GoToAsync($"entitydetail?entityId={Entity.Id}");
    }
}


