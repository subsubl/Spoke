using Spoke.Data;
using Microcharts;
using SkiaSharp;

namespace Spoke.Controls;

public partial class GraphEntityControl : ContentView
{
    private bool _isAnimating = false;

    public static readonly BindableProperty EntityProperty =
        BindableProperty.Create(nameof(Entity), typeof(GraphEntity), typeof(GraphEntityControl), null, propertyChanged: OnEntityChanged);

    public GraphEntity? Entity
    {
        get => (GraphEntity?)GetValue(EntityProperty);
        set => SetValue(EntityProperty, value);
    }

    public GraphEntityControl()
    {
        InitializeComponent();
    }

    private static void OnEntityChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (GraphEntityControl)bindable;
        control.UpdateChart();
    }

    private async void UpdateChart()
    {
        if (_isAnimating) return;
        _isAnimating = true;

        // Fade out current chart
        await ChartView.FadeTo(0.3, 200, Easing.CubicOut);

        if (Entity == null || Entity.DataPoints.Count == 0)
        {
            // Show empty chart with animation
            ChartView.Chart = new LineChart
            {
                Entries = new[] { new ChartEntry(0) { Label = "No data" } },
                LineMode = LineMode.Straight,
                PointMode = PointMode.Circle,
                PointSize = 10,
                LabelTextSize = 30,
                BackgroundColor = SKColor.Parse("#00FFFFFF")
            };
        }
        else
        {
            // Convert data points to chart entries with staggered animation effect
            var entries = Entity.DataPoints.Select((dp, index) => new ChartEntry((float)dp.Value)
            {
                Label = dp.Timestamp.ToString("HH:mm"),
                ValueLabel = dp.Value.ToString("F1"),
                Color = SKColor.Parse("#2196F3")
            }).ToArray();

            // Create chart based on graph type
            Chart? chart = Entity.GraphType.ToLower() switch
            {
                "line" => new LineChart
                {
                    Entries = entries,
                    LineMode = LineMode.Straight,
                    LineSize = 3,
                    PointMode = PointMode.Circle,
                    PointSize = 8,
                    LabelTextSize = 24,
                    ValueLabelOrientation = Orientation.Horizontal,
                    BackgroundColor = SKColor.Parse("#00FFFFFF"),
                    LabelOrientation = Orientation.Horizontal,
                    ValueLabelTextSize = 20
                },
                "bar" => new BarChart
                {
                    Entries = entries,
                    LabelTextSize = 24,
                    ValueLabelOrientation = Orientation.Horizontal,
                    BackgroundColor = SKColor.Parse("#00FFFFFF"),
                    LabelOrientation = Orientation.Horizontal,
                    ValueLabelTextSize = 20
                },
                "area" => new LineChart
                {
                    Entries = entries,
                    LineMode = LineMode.Straight,
                    LineSize = 3,
                    PointMode = PointMode.None,
                    LabelTextSize = 24,
                    ValueLabelOrientation = Orientation.Horizontal,
                    BackgroundColor = SKColor.Parse("#00FFFFFF"),
                    LabelOrientation = Orientation.Horizontal,
                    ValueLabelTextSize = 20,
                    LineAreaAlpha = 50
                },
                _ => new LineChart
                {
                    Entries = entries,
                    LineMode = LineMode.Straight,
                    BackgroundColor = SKColor.Parse("#00FFFFFF")
                }
            };

            ChartView.Chart = chart;
        }

        // Fade in new chart
        await ChartView.FadeTo(1.0, 300, Easing.CubicIn);
        
        _isAnimating = false;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        UpdateChart();
    }

    private async void OnTapped(object sender, EventArgs e)
    {
        if (Entity == null) return;
        
        // Add tap animation with scale effect
        await this.ScaleTo(0.95, 100, Easing.CubicOut);
        await this.ScaleTo(1.0, 100, Easing.CubicOut);
        
        // Navigate to entity details
        await Shell.Current.GoToAsync($"entitydetail?entityId={Entity.Id}");
    }
}


