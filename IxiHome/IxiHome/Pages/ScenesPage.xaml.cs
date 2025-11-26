using CommunityToolkit.Mvvm.ComponentModel;
using IxiHome.Data;

namespace IxiHome.Pages;

public partial class ScenesPage : ContentPage
{
    public ScenesPage()
    {
        InitializeComponent();
        BindingContext = new ScenesViewModel();
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
        LoadScenes();
    }

    private void LoadScenes()
    {
        Scenes = SceneManager.Instance.GetScenes();
    }
}