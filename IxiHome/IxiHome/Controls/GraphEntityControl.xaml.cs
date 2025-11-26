using IxiHome.Data;
using Microcharts;
using SkiaSharp;

namespace IxiHome.Controls;

public partial class GraphEntityControl : ContentView
{
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

    private void UpdateChart()
    {
        if (Entity == null || Entity.DataPoints.Count == 0)
        {
            // Show empty chart
            ChartView.Chart = new LineChart
            {
                Entries = new[] { new ChartEntry(0) { Label = "No data" } },
                LineMode = LineMode.Straight,
                PointMode = PointMode.Circle,
                PointSize = 10,
                LabelTextSize = 30,
                BackgroundColor = SKColor.Parse("#00FFFFFF")
            };
            return;
        }

        // Convert data points to chart entries
        var entries = Entity.DataPoints.Select(dp => new ChartEntry((float)dp.Value)
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

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        UpdateChart();
    }

    private async void OnTapped(object sender, EventArgs e)
    {
        if (Entity == null) return;
        
        // Navigate to entity details
        await Shell.Current.GoToAsync($"entitydetail?entityId={Entity.Id}");
    }
}
