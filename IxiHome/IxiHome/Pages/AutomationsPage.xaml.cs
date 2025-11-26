using CommunityToolkit.Mvvm.ComponentModel;
using IxiHome.Data;

namespace IxiHome.Pages;

public partial class AutomationsPage : ContentPage
{
    public AutomationsPage()
    {
        InitializeComponent();
        BindingContext = new AutomationsViewModel();
    }

    private async void OnAddAutomationClicked(object sender, EventArgs e)
    {
        // Navigate to automation creation page (to be implemented)
        await DisplayAlert("Add Automation", "Automation creation will be implemented in the next update", "OK");
    }

    private void OnAutomationToggled(object sender, ToggledEventArgs e)
    {
        if (sender is Switch toggle && toggle.BindingContext is Automation automation)
        {
            automation.IsEnabled = e.Value;
            // The automation manager will handle the enabled/disabled state
        }
    }
}

public partial class AutomationsViewModel : ObservableObject
{
    [ObservableProperty]
    private IEnumerable<Automation> automations = Array.Empty<Automation>();

    public AutomationsViewModel()
    {
        LoadAutomations();
    }

    private void LoadAutomations()
    {
        Automations = AutomationManager.Instance.GetAutomations();
    }
}