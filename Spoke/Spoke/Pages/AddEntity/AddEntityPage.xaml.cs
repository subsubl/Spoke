using IxiHome.Meta;
using IxiHome.Network;
using System.Collections.ObjectModel;

namespace Spoke.Pages.AddEntity;

public partial class AddEntityPage : ContentPage
{
    private ObservableCollection<HomeAssistantEntity> _allEntities = new();
    private ObservableCollection<HomeAssistantEntity> _filteredEntities = new();

    public AddEntityPage()
    {
        InitializeComponent();
        AvailableEntitiesCollection.ItemsSource = _filteredEntities;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Auto-load if QuIXI is connected
        if (Node.Instance.quixiClient != null && Node.Instance.quixiClient.IsConnected)
        {
            await LoadEntitiesAsync();
        }
    }

    private async void OnLoadEntitiesClicked(object sender, EventArgs e)
    {
        await LoadEntitiesAsync();
    }

    private async Task LoadEntitiesAsync()
    {
        if (Node.Instance.quixiClient == null)
        {
            await DisplayAlert("Error", "QuIXI client is not configured. Please configure in Settings.", "OK");
            return;
        }

        LoadButton.IsEnabled = false;
        LoadButton.Text = "Loading...";

        try
        {
            var entities = await Node.Instance.quixiClient.GetEntitiesAsync();
            
            _allEntities.Clear();
            _filteredEntities.Clear();
            
            foreach (var entity in entities)
            {
                _allEntities.Add(entity);
                _filteredEntities.Add(entity);
            }

            LoadButton.Text = $"Loaded {entities.Count} entities";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load entities: {ex.Message}", "OK");
            LoadButton.Text = "Load Entities from QuIXI";
        }
        finally
        {
            LoadButton.IsEnabled = true;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower() ?? "";
        
        _filteredEntities.Clear();
        
        if (string.IsNullOrWhiteSpace(searchText))
        {
            foreach (var entity in _allEntities)
            {
                _filteredEntities.Add(entity);
            }
        }
        else
        {
            foreach (var entity in _allEntities)
            {
                if (entity.FriendlyName.ToLower().Contains(searchText) ||
                    entity.EntityId.ToLower().Contains(searchText))
                {
                    _filteredEntities.Add(entity);
                }
            }
        }
    }

    private async void OnEntitySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is HomeAssistantEntity selectedEntity)
        {
            // Navigate to widget type selection
            await Shell.Current.GoToAsync($"entitytypeselector?entityId={selectedEntity.EntityId}&name={selectedEntity.FriendlyName}&domain={selectedEntity.Domain}");
            
            // Deselect
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}


