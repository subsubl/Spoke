using IxiHome.Meta;
using IXICore;
using IXICore.Meta;
using Windows.Devices.Geolocation;

namespace Spoke.Sensors;

/// <summary>
/// Manages location sensor data collection
/// </summary>
public class LocationSensorManager
{
    private readonly Geolocator _geolocator;
    private bool _isEnabled = false;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            if (_isEnabled)
            {
                StartLocationUpdates();
            }
            else
            {
                StopLocationUpdates();
            }
        }
    }

    public Windows.Devices.Geolocation.Geoposition? LastLocation { get; private set; }

    public event EventHandler<Geoposition>? LocationUpdated;

    public LocationSensorManager()
    {
        _geolocator = new Geolocator
        {
            DesiredAccuracy = PositionAccuracy.Default,
            MovementThreshold = 100, // Update every 100 meters
            ReportInterval = 300000 // 5 minutes
        };

        _geolocator.PositionChanged += OnPositionChanged;
        _geolocator.StatusChanged += OnStatusChanged;
    }

    private void OnPositionChanged(Geolocator sender, Windows.Devices.Geolocation.PositionChangedEventArgs args)
    {
        if (!_isEnabled) return;

        try
        {
            var position = args.Position;
            LastLocation = position;
            LocationUpdated?.Invoke(this, position);

            // Send to Ixian network
            SendLocationData(position);
        }
        catch (Exception ex)
        {
            Logging.error($"Location sensor error: {ex.Message}");
        }
    }

    private void OnStatusChanged(Geolocator sender, StatusChangedEventArgs args)
    {
        Logging.info($"Location status changed: {args.Status}");
    }

    private async void StartLocationUpdates()
    {
        try
        {
            var accessStatus = await Geolocator.RequestAccessAsync();

            if (accessStatus == GeolocationAccessStatus.Allowed)
            {
                Logging.info("Location access granted, starting updates");
                // Position updates are handled by the PositionChanged event
            }
            else
            {
                Logging.warn($"Location access denied: {accessStatus}");
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Failed to start location updates: {ex.Message}");
        }
    }

    private void StopLocationUpdates()
    {
        Logging.info("Location updates stopped");
        // Geolocator doesn't have a stop method, but we can ignore updates
    }

    private async void SendLocationData(Geoposition position)
    {
        if (Node.Instance.quixiClient == null) return;

        try
        {
            var data = new Dictionary<string, object>
            {
                ["latitude"] = position.Coordinate.Point.Position.Latitude,
                ["longitude"] = position.Coordinate.Point.Position.Longitude,
                ["accuracy"] = position.Coordinate.Accuracy,
                ["altitude"] = position.Coordinate.Point.Position.Altitude,
                ["timestamp"] = position.Coordinate.Timestamp.ToUnixTimeSeconds()
            };

            // Send as sensor update
            await Node.Instance.quixiClient.SendCommandAsync("sensor.location", "update", data);
        }
        catch (Exception ex)
        {
            Logging.error($"Failed to send location data: {ex.Message}");
        }
    }

    public async Task<Geoposition?> GetCurrentLocationAsync()
    {
        try
        {
            var accessStatus = await Geolocator.RequestAccessAsync();

            if (accessStatus == GeolocationAccessStatus.Allowed)
            {
                return await _geolocator.GetGeopositionAsync();
            }
            else
            {
                Logging.warn($"Location access denied: {accessStatus}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Failed to get current location: {ex.Message}");
            return null;
        }
    }
}

