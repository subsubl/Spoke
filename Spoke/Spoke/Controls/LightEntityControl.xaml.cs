using Spoke.Data;
using Spoke.Meta;
using IXICore;
using IXICore.Meta;

namespace Spoke.Controls;

public partial class LightEntityControl : ContentView
{
    public static readonly BindableProperty EntityProperty =
        BindableProperty.Create(nameof(Entity), typeof(LightEntity), typeof(LightEntityControl), null);

    public LightEntity? Entity
    {
        get => (LightEntity?)GetValue(EntityProperty);
        set => SetValue(EntityProperty, value);
    }

    public LightEntityControl()
    {
        InitializeComponent();
    }

    private async void OnPowerToggled(object sender, ToggledEventArgs e)
    {
        if (Entity == null) return;

        if (e.Value)
        {
            await SendCommandAsync("turn_on", new Dictionary<string, object>());
        }
        else
        {
            await SendCommandAsync("turn_off", new Dictionary<string, object>());
        }
    }

    private async void OnBrightnessChanged(object sender, ValueChangedEventArgs e)
    {
        if (Entity == null || !Entity.IsOn) return;

        // Debounce brightness changes
        await Task.Delay(100);
        if (Entity == null) return;

        await SendCommandAsync("turn_on", new Dictionary<string, object> { ["brightness"] = (int)e.NewValue });
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

                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Failed to control {Entity.Name}",
                        "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Exception sending command {command} for entity {Entity.Name}: {ex.Message}");

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

