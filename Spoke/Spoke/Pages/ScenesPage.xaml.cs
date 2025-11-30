using CommunityToolkit.Mvvm.ComponentModel;
using Spoke.Data;
using IXICore.Meta;
using System.IO;

namespace Spoke.Pages;

public partial class ScenesPage : ContentPage
{
    public ScenesPage()
    {
        try { File.AppendAllText("c:\\Users\\User\\Spoke\\Spoke\\nav_debug.log", DateTime.Now+" ScenesPage ctor BEFORE InitializeComponent\n"); } catch {}
        Logging.info("ScenesPage: constructor start");
        InitializeComponent();
        try { File.AppendAllText("c:\\Users\\User\\Spoke\\Spoke\\nav_debug.log", DateTime.Now+" ScenesPage ctor AFTER InitializeComponent\n"); } catch {}
        BindingContext = new ScenesViewModel();
        Logging.info("ScenesPage: constructor end");
        try { File.AppendAllText("c:\\Users\\User\\Spoke\\Spoke\\nav_debug.log", DateTime.Now+" ScenesPage ctor end\n"); } catch {}
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Logging.info("ScenesPage: OnAppearing start");
        try { File.AppendAllText("c:\\Users\\User\\Spoke\\Spoke\\nav_debug.log", DateTime.Now+" ScenesPage OnAppearing start\n"); } catch {}
        try
        {
            // Load scenes when the page appears to ensure managers are initialized
            if (BindingContext is ScenesViewModel viewModel)
            {
                viewModel.RefreshScenes();
            }
        }
        catch (Exception ex)
        {
            Logging.error($"ScenesPage: OnAppearing error: {ex}");
            try { File.AppendAllText("c:\\Users\\User\\Spoke\\Spoke\\nav_debug.log", DateTime.Now+" ScenesPage OnAppearing error: "+ex+"\n"); } catch {}
        }
        Logging.info("ScenesPage: OnAppearing end");
        try { File.AppendAllText("c:\\Users\\User\\Spoke\\Spoke\\nav_debug.log", DateTime.Now+" ScenesPage OnAppearing end\n"); } catch {}
    }

    private async void OnAddSceneClicked(object sender, EventArgs e)
    {
        // Navigate to scene creation page (to be implemented)
        await DisplayAlert("Add Scene", "Scene creation will be implemented in the next update", "OK");
    }

    private async void OnActivateSceneClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Scene scene)
        {
            try
            {
                await SceneManager.Instance.ActivateSceneAsync(scene.Id);
                await DisplayAlert("Success", $"Scene '{scene.Name}' activated", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to activate scene: {ex.Message}", "OK");
            }
        }
    }
}

public partial class ScenesViewModel : ObservableObject
{
    [ObservableProperty]
    private IEnumerable<Scene> scenes = Array.Empty<Scene>();

    public ScenesViewModel()
    {
        // Defer loading until the page appears to ensure managers are initialized
        // LoadScenes();
    }

    private void LoadScenes()
    {
        try
        {
            Scenes = SceneManager.Instance.GetScenes();
        }
        catch (Exception ex)
        {
            Logging.error($"Error loading scenes: {ex.Message}");
            Scenes = Array.Empty<Scene>();
        }
    }

    public void RefreshScenes()
    {
        LoadScenes();
        OnPropertyChanged(nameof(Scenes));
    }
}

