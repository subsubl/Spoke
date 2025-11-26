using CommunityToolkit.Mvvm.ComponentModel;
using IxiHome.Data;
using IxiHome.Widgets;

namespace IxiHome.Pages;

public partial class WidgetsPage : ContentPage
{
    public WidgetsPage()
    {
        InitializeComponent();
        BindingContext = new WidgetsViewModel();
    }

    private void OnCreateSensorWidgetClicked(object sender, EventArgs e)
    {
        try
        {
            string widgetId = $"sensor_{DateTime.Now.Ticks}";
            WidgetManager.Instance.CreateWidget<SensorWidget>(widgetId);
            RefreshActiveWidgets();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to create sensor widget: {ex.Message}", "OK");
        }
    }

    private void OnCreateSceneWidgetClicked(object sender, EventArgs e)
    {
        try
        {
            string widgetId = $"scene_{DateTime.Now.Ticks}";
            WidgetManager.Instance.CreateWidget<SceneWidget>(widgetId);
            RefreshActiveWidgets();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to create scene widget: {ex.Message}", "OK");
        }
    }

    private void OnCreateEntityWidgetClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Entity entity)
        {
            try
            {
                string widgetId = $"entity_{entity.EntityId}_{DateTime.Now.Ticks}";
                WidgetManager.Instance.CreateWidget<EntityWidget>(widgetId, entity.EntityId);
                RefreshActiveWidgets();
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Failed to create widget for {entity.Name}: {ex.Message}", "OK");
            }
        }
    }

    private void OnRemoveWidgetClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Widget widget)
        {
            WidgetManager.Instance.RemoveWidget(widget.Id);
            RefreshActiveWidgets();
        }
    }

    private void RefreshActiveWidgets()
    {
        if (BindingContext is WidgetsViewModel viewModel)
        {
            viewModel.RefreshActiveWidgets();
        }
    }
}

public partial class WidgetsViewModel : ObservableObject
{
    [ObservableProperty]
    private IEnumerable<Widget> activeWidgets = Array.Empty<Widget>();

    [ObservableProperty]
    private IEnumerable<Entity> availableEntities = Array.Empty<Entity>();

    public WidgetsViewModel()
    {
        LoadData();
    }

    private void LoadData()
    {
        AvailableEntities = EntityManager.Instance.Entities;
        RefreshActiveWidgets();
    }

    public void RefreshActiveWidgets()
    {
        ActiveWidgets = WidgetManager.Instance.GetWidgets();
    }
}