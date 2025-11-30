namespace Spoke;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute("addentity", typeof(Pages.AddEntity.AddEntityPage));
        Routing.RegisterRoute("entitydetail", typeof(Pages.EntityDetail.EntityDetailPage));

        // Attach navigation logging for debugging
        this.Navigating += AppShell_Navigating;
        this.Navigated += AppShell_Navigated;
    }

    private void AppShell_Navigating(object? sender, ShellNavigatingEventArgs e)
    {

    }

    private void AppShell_Navigated(object? sender, ShellNavigatedEventArgs e)
    {

    }
}


