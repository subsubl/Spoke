using IxiHome.Data;
using Microsoft.Maui.Graphics;

namespace IxiHome.Controls;

public partial class GaugeEntityControl : ContentView
{
    private double _currentPercentage = 0;
    private double _targetPercentage = 0;
    private bool _isAnimating = false;

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
        GaugeGraphicsView.Drawable = new GaugeDrawable(this);
    }

    private static void OnEntityChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (GaugeEntityControl)bindable;
        control.UpdateGauge();
    }

    private async void UpdateGauge()
    {
        if (Entity == null) return;

        // Update value display with animation
        await AnimateValueChange(Entity.Value);

        // Calculate target percentage
        _targetPercentage = Entity.GetPercentage();

        // Animate the gauge progress
        if (!_isAnimating)
        {
            await AnimateGaugeProgress();
        }

        // Update colors based on percentage
        UpdateGaugeColors();
        GaugeGraphicsView.Invalidate();
    }

    private async Task AnimateValueChange(double newValue)
    {
        // Simple fade animation for value change
        await ValueLabel.FadeTo(0.3, 150, Easing.Linear);
        ValueLabel.Text = newValue.ToString("F1");
        await ValueLabel.FadeTo(1.0, 150, Easing.Linear);
    }

    private async Task AnimateGaugeProgress()
    {
        _isAnimating = true;
        
        // Smooth animation from current to target percentage
        var animation = new Animation(v => 
        {
            _currentPercentage = v;
            GaugeGraphicsView.Invalidate();
        }, _currentPercentage, _targetPercentage, Easing.CubicInOut);

        animation.Commit(this, "GaugeProgress", length: 800);
        
        await Task.Delay(800);
        _isAnimating = false;
    }

    private void UpdateGaugeColors()
    {
        if (_targetPercentage >= 80)
        {
            ((GaugeDrawable)GaugeGraphicsView.Drawable).ProgressColor = Colors.Red;
        }
        else if (_targetPercentage >= 60)
        {
            ((GaugeDrawable)GaugeGraphicsView.Drawable).ProgressColor = Colors.Orange;
        }
        else if (_targetPercentage >= 40)
        {
            ((GaugeDrawable)GaugeGraphicsView.Drawable).ProgressColor = Colors.Yellow;
        }
        else
        {
            ((GaugeDrawable)GaugeGraphicsView.Drawable).ProgressColor = Colors.Green;
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
        
        // Add tap animation
        await this.ScaleTo(0.95, 100, Easing.CubicOut);
        await this.ScaleTo(1.0, 100, Easing.CubicOut);
        
        // Navigate to entity details
        await Shell.Current.GoToAsync($"entitydetail?entityId={Entity.Id}");
    }

    // Custom drawable for circular progress
    private class GaugeDrawable : IDrawable
    {
        private readonly GaugeEntityControl _control;

        public Color ProgressColor { get; set; } = Colors.Green;

        public GaugeDrawable(GaugeEntityControl control)
        {
            _control = control;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            float centerX = dirtyRect.Width / 2;
            float centerY = dirtyRect.Height / 2;
            float radius = Math.Min(centerX, centerY) - 10;

            // Draw background circle
            canvas.StrokeColor = Colors.LightGray;
            canvas.StrokeSize = 8;
            canvas.DrawCircle(centerX, centerY, radius);

            // Draw progress arc
            if (_control._currentPercentage > 0)
            {
                canvas.StrokeColor = ProgressColor;
                canvas.StrokeSize = 8;
                
                // Calculate arc angles (start from top, clockwise)
                float startAngle = -90; // Start from top
                float sweepAngle = (float)(_control._currentPercentage / 100.0 * 360);
                
                canvas.DrawArc(centerX - radius, centerY - radius, radius * 2, radius * 2, 
                              startAngle, sweepAngle, false, false);
            }
        }
    }
}
