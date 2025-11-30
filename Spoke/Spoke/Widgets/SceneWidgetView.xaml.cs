using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IxiHome.Data;
using System.Collections.ObjectModel;

namespace Spoke.Widgets;

public partial class SceneWidgetView : ContentPage
{
    public SceneWidgetView()
    {
        InitializeComponent();
        BindingContext = new SceneWidgetViewModel();
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        // Close the widget window
        WidgetManager.Instance.CloseWidget(this);
    }
}

public class SceneWidgetViewModel : ObservableObject
{
    public ObservableCollection<SceneItem> Scenes { get; } = new();

    public SceneWidgetViewModel()
    {
        LoadScenes();
    }

    private void LoadScenes()
    {
        // Load scenes from SceneManager
        if (SceneManager.Instance != null)
        {
            var scenes = SceneManager.Instance.GetScenes();
            foreach (var scene in scenes)
            {
                Scenes.Add(new SceneItem(scene));
            }
        }
    }
}

public class SceneItem
{
    public string Name { get; set; }
    public IAsyncRelayCommand ActivateCommand { get; }

    private readonly Scene _scene;

    public SceneItem(Scene scene)
    {
        _scene = scene;
        Name = scene.Name;
        ActivateCommand = new AsyncRelayCommand(ActivateSceneAsync);
    }

    private async Task ActivateSceneAsync()
    {
        try
        {
            await SceneManager.Instance.ActivateSceneAsync(_scene.Id);
        }
        catch (Exception ex)
        {
            // Handle error - could show toast or log
            Console.WriteLine($"Failed to activate scene '{Name}': {ex.Message}");
        }
    }
}

