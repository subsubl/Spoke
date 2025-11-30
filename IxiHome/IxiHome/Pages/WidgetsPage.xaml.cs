using CommunityToolkit.Mvvm.ComponentModel;
using IxiHome.Data;
using IxiHome.Widgets;
using IXICore.Meta;
using System.IO;

namespace IxiHome.Pages;

public partial class WidgetsPage : ContentPage
{
    public WidgetsPage()
    {
           try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" WidgetsPage ctor BEFORE InitializeComponent\n"); } catch {}
           Logging.info("WidgetsPage: constructor start");
           InitializeComponent();
           try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" WidgetsPage ctor AFTER InitializeComponent\n"); } catch {}
           BindingContext = new WidgetsViewModel();
           Logging.info("WidgetsPage: constructor end");
           try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" WidgetsPage ctor end\n"); } catch {}
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Logging.info("WidgetsPage: OnAppearing start");
        try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" WidgetsPage OnAppearing start\n"); } catch {}
        try
        {
            // Load data when the page appears to ensure managers are initialized
            if (BindingContext is WidgetsViewModel viewModel)
            {
                viewModel.RefreshData();
            }
        }
        catch (Exception ex)
        {
            Logging.error($"WidgetsPage: OnAppearing error: {ex}");
            try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" WidgetsPage OnAppearing error: "+ex+"\n"); } catch {}
        }
        Logging.info("WidgetsPage: OnAppearing end");
        try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" WidgetsPage OnAppearing end\n"); } catch {}
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
        // Defer loading until the page appears to ensure managers are initialized
        // LoadData();
    }

    private void LoadData()
    {
        try
        {
            AvailableEntities = EntityManager.Instance.Entities;
            RefreshActiveWidgets();
        }
        catch (Exception ex)
        {
            Logging.error($"Error loading widget data: {ex.Message}");
            AvailableEntities = Array.Empty<Entity>();
            ActiveWidgets = Array.Empty<Widget>();
        }
    }

    public void RefreshData()
    {
        LoadData();
        OnPropertyChanged(nameof(AvailableEntities));
        OnPropertyChanged(nameof(ActiveWidgets));
    }

    public void RefreshActiveWidgets()
    {
        ActiveWidgets = WidgetManager.Instance.GetWidgets();
    }
}