using IxiHome.Data;
using IxiHome.Meta;

namespace Spoke.Pages.EntityDetail;

[QueryProperty(nameof(EntityId), "entityId")]
public partial class EntityDetailPage : ContentPage
{
    private Entity? _entity;
    
    public string EntityId { get; set; } = "";

    public EntityDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadEntity();
    }

    private void LoadEntity()
    {
        _entity = EntityManager.Instance.GetEntityById(EntityId);
        
        if (_entity == null)
        {
            DisplayAlert("Error", "Entity not found", "OK");
            Shell.Current.GoToAsync("..");
            return;
        }

        // Display entity info
        EntityIconLabel.Text = string.IsNullOrEmpty(_entity.Icon) ? "🏠" : _entity.Icon;
        EntityNameLabel.Text = _entity.Name;
        EntityStateLabel.Text = _entity.GetDisplayState();
        EntityIdLabel.Text = _entity.EntityId;

        // Populate edit fields
        EntityNameEntry.Text = _entity.Name;
        EntityIconEntry.Text = _entity.Icon;

        // Show controls for toggle entities
        if (_entity is ToggleEntity)
        {
            ControlFrame.IsVisible = true;
            ToggleButton.IsVisible = true;
        }
    }

    private async void OnToggleClicked(object sender, EventArgs e)
    {
        if (_entity == null || Node.Instance.quixiClient == null) return;

        try
        {
            await Node.Instance.quixiClient.ToggleAsync(_entity.EntityId);
            await DisplayAlert("Success", "Command sent successfully", "OK");
            
            // Refresh state
            var newState = await Node.Instance.quixiClient.GetEntityStateAsync(_entity.EntityId);
            if (newState != null)
            {
                _entity.UpdateState(newState.State, newState.Attributes);
                EntityStateLabel.Text = _entity.GetDisplayState();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to toggle entity: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_entity == null) return;

        _entity.Name = EntityNameEntry.Text ?? _entity.Name;
        _entity.Icon = EntityIconEntry.Text ?? _entity.Icon;

        await EntityManager.Instance.UpdateEntityAsync(_entity);
        await DisplayAlert("Success", "Entity updated successfully", "OK");
        
        // Refresh display
        EntityIconLabel.Text = string.IsNullOrEmpty(_entity.Icon) ? "🏠" : _entity.Icon;
        EntityNameLabel.Text = _entity.Name;
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_entity == null) return;

        bool confirm = await DisplayAlert("Confirm Delete", 
            $"Are you sure you want to delete {_entity.Name}?", 
            "Delete", "Cancel");

        if (confirm)
        {
            await EntityManager.Instance.RemoveEntityAsync(_entity);
            await Shell.Current.GoToAsync("..");
        }
    }
}


