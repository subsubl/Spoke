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
        try { System.IO.File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now + " Navigating: " + e.Target?.Location + "\n"); } catch {}
    }

    private void AppShell_Navigated(object? sender, ShellNavigatedEventArgs e)
    {
        try { System.IO.File.AppendAllText("c:\\Users\\User\\IxiHome\\IxiHome\\nav_debug.log", DateTime.Now + " Navigated: " + e.Current?.Location + "\n"); } catch {}
    }
}


