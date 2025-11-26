namespace IxiHome;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute("addentity", typeof(Pages.AddEntity.AddEntityPage));
        Routing.RegisterRoute("entitydetail", typeof(Pages.EntityDetail.EntityDetailPage));
    }
}
