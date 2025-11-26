using IxiHome.Meta;

namespace IxiHome.Pages;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        VersionLabel.Text = $"Version {Config.version}";
    }

    private async void OnVisitWebsiteClicked(object sender, EventArgs e)
    {
        await Launcher.OpenAsync("https://www.ixian.io");
    }
}
