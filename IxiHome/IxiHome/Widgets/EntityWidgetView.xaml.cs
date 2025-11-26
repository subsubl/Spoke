using IxiHome.Data;

namespace IxiHome.Widgets;

public partial class EntityWidgetView : ContentPage
{
    private EntityWidget? _widget;

    public EntityWidgetView()
    {
        InitializeComponent();
        BindingContextChanged += OnBindingContextChanged;
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (BindingContext is EntityWidget widget)
        {
            _widget = widget;
            UpdateEntityControl();
        }
    }

    private void UpdateEntityControl()
    {
        if (_widget?.Entity == null)
        {
            EntityControlContainer.Content = new Label
            {
                Text = "No entity selected",
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            return;
        }

        // Create appropriate control based on entity type
        View control = _widget.Entity switch
        {
            LightEntity lightEntity => CreateLightControl(lightEntity),
            ToggleEntity toggleEntity => CreateToggleControl(toggleEntity),
            SensorEntity sensorEntity => CreateSensorControl(sensorEntity),
            _ => CreateGenericControl(_widget.Entity)
        };

        EntityControlContainer.Content = control;
    }

    private View CreateToggleControl(ToggleEntity entity)
    {
        var button = new Button
        {
            Text = entity.IsOn ? "Turn OFF" : "Turn ON",
            BackgroundColor = entity.IsOn ? Colors.Orange : Colors.Green,
            TextColor = Colors.White,
            Style = (Style)Resources["WidgetButtonStyle"]
        };

        button.Clicked += async (s, e) =>
        {
            try
            {
                bool success = await Meta.Node.Instance.quixiClient.ToggleAsync(entity.EntityId);
                if (success)
                {
                    button.Text = entity.IsOn ? "Turn OFF" : "Turn ON";
                    button.BackgroundColor = entity.IsOn ? Colors.Orange : Colors.Green;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to toggle {entity.Name}: {ex.Message}", "OK");
            }
        };

        return button;
    }

    private View CreateLightControl(LightEntity entity)
    {
        var layout = new VerticalStackLayout { Spacing = 4 };

        // Brightness slider
        var brightnessLabel = new Label
        {
            Text = $"Brightness: {entity.Brightness}%",
            FontSize = 10,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var slider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Value = entity.Brightness,
            HeightRequest = 20
        };

        slider.ValueChanged += async (s, e) =>
        {
            brightnessLabel.Text = $"Brightness: {e.NewValue:F0}%";
            // TODO: Send brightness command
        };

        // On/Off button
        var button = new Button
        {
            Text = entity.IsOn ? "Turn OFF" : "Turn ON",
            BackgroundColor = entity.IsOn ? Colors.Orange : Colors.Green,
            TextColor = Colors.White,
            Style = (Style)Resources["WidgetButtonStyle"]
        };

        button.Clicked += async (s, e) =>
        {
            try
            {
                string command = entity.IsOn ? "turn_off" : "turn_on";
                var success = await Meta.Node.Instance.quixiClient.SendCommandAsync(entity.EntityId, command, null);
                if (success)
                {
                    button.Text = entity.IsOn ? "Turn OFF" : "Turn ON";
                    button.BackgroundColor = entity.IsOn ? Colors.Orange : Colors.Green;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to control {entity.Name}: {ex.Message}", "OK");
            }
        };

        layout.Children.Add(brightnessLabel);
        layout.Children.Add(slider);
        layout.Children.Add(button);

        return layout;
    }

    private View CreateSensorControl(SensorEntity entity)
    {
        var layout = new VerticalStackLayout { Spacing = 4 };

        var valueLabel = new Label
        {
            Text = $"{entity.Value:F1}",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var unitLabel = new Label
        {
            Text = entity.Unit,
            FontSize = 12,
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = Colors.Gray
        };

        layout.Children.Add(valueLabel);
        layout.Children.Add(unitLabel);

        return layout;
    }

    private View CreateGenericControl(Entity entity)
    {
        return new Label
        {
            Text = $"{entity.State}",
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        if (_widget != null)
        {
            WidgetManager.Instance.RemoveWidget(_widget.Id);
        }
    }
}