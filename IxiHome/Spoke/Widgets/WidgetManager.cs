using IXICore.Meta;
using IxiHome.Data;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Spoke.Widgets;

/// <summary>
/// Manages desktop widgets for quick access to smart home controls
/// </summary>
public class WidgetManager
{
    private static WidgetManager? _instance;
    public static WidgetManager Instance => _instance ??= new WidgetManager();

    private readonly Dictionary<string, Widget> _widgets = new();
    private readonly Dictionary<string, ContentPage> _widgetPages = new();
    private readonly object _widgetsLock = new();

    private CancellationTokenSource? _updateCts;
    private Task? _updateTask;

    private WidgetManager() { }

    /// <summary>
    /// Start the widget manager
    /// </summary>
    public void Start()
    {
        if (_updateTask != null)
            return;

        _updateCts = new CancellationTokenSource();
        _updateTask = Task.Run(() => UpdateWidgetsAsync(_updateCts.Token));

        Logging.info("WidgetManager started");
    }

    /// <summary>
    /// Stop the widget manager
    /// </summary>
    public void Stop()
    {
        _updateCts?.Cancel();
        _updateTask?.Wait();

        // Close all widget pages
        foreach (var page in _widgetPages.Values)
        {
            // Pages will be closed when the app closes
        }
        _widgetPages.Clear();

        _updateTask = null;
        _updateCts = null;

        Logging.info("WidgetManager stopped");
    }

    /// <summary>
    /// Create and show a widget
    /// </summary>
    public void CreateWidget<T>(string widgetId, params object[] args) where T : Widget
    {
        lock (_widgetsLock)
        {
            if (_widgets.ContainsKey(widgetId))
            {
                Logging.warn($"Widget {widgetId} already exists");
                return;
            }

            Widget widget = (T)Activator.CreateInstance(typeof(T), args)!;
            widget.Id = widgetId;

            _widgets[widgetId] = widget;
            ShowWidgetPage(widget);
        }
    }

    /// <summary>
    /// Remove a widget
    /// </summary>
    public void RemoveWidget(string widgetId)
    {
        lock (_widgetsLock)
        {
            if (_widgets.TryGetValue(widgetId, out var widget))
            {
                _widgets.Remove(widgetId);

                if (_widgetPages.TryGetValue(widgetId, out var page))
                {
                    // Navigate back if this page is currently displayed
                    if (Application.Current?.MainPage?.Navigation?.NavigationStack?.LastOrDefault() == page)
                    {
                        Application.Current.MainPage.Navigation.PopAsync();
                    }
                    _widgetPages.Remove(widgetId);
                }
            }
        }
    }

    /// <summary>
    /// Get all widgets
    /// </summary>
    public IEnumerable<Widget> GetWidgets()
    {
        lock (_widgetsLock)
        {
            return _widgets.Values.ToList();
        }
    }

    /// <summary>
    /// Close a widget by its view
    /// </summary>
    public void CloseWidget(ContentPage page)
    {
        var widgetId = _widgetPages.FirstOrDefault(x => x.Value == page).Key;
        if (widgetId != null)
        {
            RemoveWidget(widgetId);
        }
    }

    /// <summary>
    /// Show a widget page
    /// </summary>
    private async void ShowWidgetPage(Widget widget)
    {
        var page = widget.GetView() as ContentPage;
        if (page == null)
        {
            Logging.error($"Widget {widget.Id} returned null or non-ContentPage view");
            return;
        }

        // Store reference
        _widgetPages[widget.Id] = page;

        // Navigate to the widget page modally
        if (Application.Current?.MainPage is NavigationPage navPage)
        {
            await navPage.PushAsync(page);
        }
        else if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.Navigation.PushAsync(page);
        }
        else
        {
            Logging.error("Cannot show widget: no main page available");
        }
    }

    /// <summary>
    /// Update all widgets periodically
    /// </summary>
    private async Task UpdateWidgetsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;

                lock (_widgetsLock)
                {
                    foreach (var widget in _widgets.Values)
                    {
                        widget.Update();
                    }
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logging.error($"Error updating widgets: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Create default widgets for common entities
    /// </summary>
    public void CreateDefaultWidgets()
    {
        // Create sensor widget
        CreateWidget<SensorWidget>("default_sensor");

        // Create scene widget
        CreateWidget<SceneWidget>("default_scene");

        // Create entity widgets for common entities
        var entities = EntityManager.Instance.Entities.Take(3); // Limit to 3 for now
        int index = 0;
        foreach (var entity in entities)
        {
            CreateWidget<EntityWidget>($"entity_{index}", entity.EntityId);
            index++;
        }
    }

    /// <summary>
    /// Save widget positions and settings
    /// </summary>
    public void SaveWidgetSettings()
    {
        // TODO: Implement persistent storage for widget positions and settings
        Logging.info("WidgetManager: SaveWidgetSettings not implemented yet");
    }

    /// <summary>
    /// Load widget settings
    /// </summary>
    public void LoadWidgetSettings()
    {
        // TODO: Implement loading widget settings
        Logging.info("WidgetManager: LoadWidgetSettings not implemented yet");
    }
}

