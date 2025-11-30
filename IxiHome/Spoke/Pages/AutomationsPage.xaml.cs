using CommunityToolkit.Mvvm.ComponentModel;
using IxiHome.Data;
using IXICore.Meta;
using System.IO;

namespace Spoke.Pages;

public partial class AutomationsPage : ContentPage
{
    public AutomationsPage()
    {
        try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" AutomationsPage ctor BEFORE InitializeComponent\n"); } catch {}
        Logging.info("AutomationsPage: constructor start");
        InitializeComponent();
        try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" AutomationsPage ctor AFTER InitializeComponent\n"); } catch {}
        BindingContext = new AutomationsViewModel();
        Logging.info("AutomationsPage: constructor end");
        try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" AutomationsPage ctor end\n"); } catch {}
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Logging.info("AutomationsPage: OnAppearing start");
        try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" AutomationsPage OnAppearing start\n"); } catch {}
        try
        {
            // Load automations when the page appears to ensure managers are initialized
            if (BindingContext is AutomationsViewModel viewModel)
            {
                viewModel.RefreshAutomations();
            }
        }
        catch (Exception ex)
        {
            Logging.error($"AutomationsPage: OnAppearing error: {ex}");
            try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" AutomationsPage OnAppearing error: "+ex+"\n"); } catch {}
        }
        Logging.info("AutomationsPage: OnAppearing end");
        try { File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now+" AutomationsPage OnAppearing end\n"); } catch {}
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
        // Defer loading until the page appears to ensure managers are initialized
        // LoadAutomations();
    }

    private void LoadAutomations()
    {
        try
        {
            Automations = AutomationManager.Instance.GetAutomations();
        }
        catch (Exception ex)
        {
            Logging.error($"Error loading automations: {ex.Message}");
            Automations = Array.Empty<Automation>();
        }
    }

    public void RefreshAutomations()
    {
        LoadAutomations();
        OnPropertyChanged(nameof(Automations));
    }
}

