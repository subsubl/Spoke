using Spoke.Data;
using Spoke.Meta;
using IXICore;
using IXICore.Meta;

namespace Spoke.Controls;

public partial class ToggleEntityControl : ContentView
{
    public static readonly BindableProperty EntityProperty =
        BindableProperty.Create(nameof(Entity), typeof(ToggleEntity), typeof(ToggleEntityControl), null);

    public ToggleEntity? Entity
    {
        get => (ToggleEntity?)GetValue(EntityProperty);
        set => SetValue(EntityProperty, value);
    }

    public ToggleEntityControl()
    {
        InitializeComponent();
    }

    private async void OnTapped(object sender, EventArgs e)
    {
        if (Entity == null) return;
        
        // Toggle the entity
        await ToggleEntityAsync();
    }

    private async void OnSwitchToggled(object sender, ToggledEventArgs e)
    {
        if (Entity == null) return;
        
        // Prevent feedback loop
        var switchControl = (Switch)sender;
        if (switchControl.IsToggled == Entity.IsOn)
        {
            return;
        }
        
        await ToggleEntityAsync();
    }

    private async Task ToggleEntityAsync()
    {
        if (Entity == null || Node.Instance.quixiClient == null)
        {
            Logging.warn("Cannot toggle entity - Entity or QuixiClient is null");
            return;
        }

        try
        {
            bool success = await Node.Instance.quixiClient.ToggleAsync(Entity.EntityId);
            
            if (success)
            {
                // Update local state (will be confirmed by state sync)
                Entity.IsOn = !Entity.IsOn;
                Entity.State = Entity.IsOn ? "on" : "off";
                
                Logging.info($"Toggled entity {Entity.Name} to {Entity.State}");
            }
            else
            {
                Logging.error($"Failed to toggle entity {Entity.Name}");
                
                // Show error to user
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error", 
                        $"Failed to toggle {Entity.Name}", 
                        "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Exception toggling entity {Entity.Name}: {ex.Message}");
            
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error", 
                    $"Error: {ex.Message}", 
                    "OK");
            }
        }
    }
}


