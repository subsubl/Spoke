using Spoke.Data;
using Spoke.Meta;
using IXICore;
using IXICore.Meta;

namespace Spoke.Controls;

public partial class ClimateEntityControl : ContentView
{
    public static readonly BindableProperty EntityProperty =
        BindableProperty.Create(nameof(Entity), typeof(ClimateEntity), typeof(ClimateEntityControl), null);

    public ClimateEntity? Entity
    {
        get => (ClimateEntity?)GetValue(EntityProperty);
        set => SetValue(EntityProperty, value);
    }

    public ClimateEntityControl()
    {
        InitializeComponent();
    }

    private async void OnTargetTemperatureChanged(object sender, ValueChangedEventArgs e)
    {
        if (Entity == null) return;

        // Debounce temperature changes
        await Task.Delay(200);
        if (Entity == null) return;

        await SendCommandAsync("set_temperature", new Dictionary<string, object> { ["temperature"] = e.NewValue });
    }

    private async Task SendCommandAsync(string command, Dictionary<string, object> parameters)
    {
        if (Entity == null || Node.Instance.quixiClient == null)
        {
            Logging.warn("Cannot send command - Entity or QuixiClient is null");
            return;
        }

        try
        {
            bool success = await Node.Instance.quixiClient.SendCommandAsync(Entity.EntityId, command, parameters);

            if (!success)
            {
                Logging.error($"Failed to send command {command} for entity {Entity.Name}");

                if (App.appWindow?.Page != null)
                {
                    await App.appWindow.Page.DisplayAlert(
                        "Error", 
                        $"Failed to set temperature for {Entity.Name}", 
                        "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Exception sending command {command} for entity {Entity.Name}: {ex.Message}");

            if (App.appWindow?.Page != null)
            {
                await App.appWindow.Page.DisplayAlert(
                    "Error", 
                    $"Error: {ex.Message}", 
                    "OK");
            }
        }
    }
}

