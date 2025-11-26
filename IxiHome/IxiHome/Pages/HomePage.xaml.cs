using IxiHome.Data;

namespace IxiHome.Pages;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        LoadEntities();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadEntities();
    }

    private async void LoadEntities()
    {
        await EntityManager.Instance.LoadEntitiesAsync();
        EntitiesCollection.ItemsSource = EntityManager.Instance.Entities;
    }

    private async void OnAddEntityClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("addentity");
    }
}
