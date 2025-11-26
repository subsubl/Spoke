using IxiHome.Meta;
using IXICore;
using IXICore.Meta;
using Windows.Devices.Power;
using Windows.System.Power;

namespace IxiHome.Sensors;

/// <summary>
/// Manages battery sensor data collection
/// </summary>
public class BatterySensorManager
{
    private bool _isEnabled = false;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            if (_isEnabled)
            {
                StartBatteryMonitoring();
            }
            else
            {
                StopBatteryMonitoring();
            }
        }
    }

    public event EventHandler<Windows.Devices.Power.BatteryReport>? BatteryUpdated;

    public Windows.Devices.Power.BatteryReport? LastBatteryInfo { get; private set; }

    public int LowBatteryThreshold { get; set; } = 20; // Alert when below 20%
    private bool _lowBatteryAlertShown = false;

    public BatterySensorManager()
    {
        // Battery status changes are not directly monitored, we'll poll
    }

    private void StartBatteryMonitoring()
    {
        Logging.info("Battery monitoring started");
        // Initial report
        ReportBatteryStatus();

        // Set up periodic reporting
        Task.Run(async () =>
        {
            while (_isEnabled)
            {
                await Task.Delay(TimeSpan.FromMinutes(5)); // Report every 5 minutes
                if (_isEnabled)
                {
                    ReportBatteryStatus();
                }
            }
        });
    }

    private void StopBatteryMonitoring()
    {
        Logging.info("Battery monitoring stopped");
    }

    private void ReportBatteryStatus()
    {
        try
        {
            var batteryReport = Windows.Devices.Power.Battery.AggregateBattery.GetReport();

            if (batteryReport != null)
            {
                LastBatteryInfo = batteryReport;
                BatteryUpdated?.Invoke(this, batteryReport);
                SendBatteryData(batteryReport);

                // Check battery thresholds
                CheckBatteryThresholds(batteryReport);
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Battery sensor error: {ex.Message}");
        }
    }

    private void CheckBatteryThresholds(Windows.Devices.Power.BatteryReport batteryReport)
    {
        try
        {
            double percentage = 0;
            if ((batteryReport.FullChargeCapacityInMilliwattHours ?? 0) > 0)
            {
                percentage = (double)batteryReport.RemainingCapacityInMilliwattHours.Value / batteryReport.FullChargeCapacityInMilliwattHours.Value * 100;
            }

            if (percentage <= LowBatteryThreshold && !_lowBatteryAlertShown)
            {
                Notifications.NotificationManager.Instance.ShowSensorAlertNotification(
                    "Battery",
                    $"Battery low: {percentage:F0}%",
                    "Please connect charger");
                _lowBatteryAlertShown = true;
            }
            else if (percentage > LowBatteryThreshold + 5 && _lowBatteryAlertShown)
            {
                // Reset alert when battery recovers
                _lowBatteryAlertShown = false;
            }
        }
        catch (Exception ex)
        {
            Logging.error($"Error checking battery thresholds: {ex.Message}");
        }
    }

    private async void SendBatteryData(BatteryReport report)
    {
        if (Node.Instance.quixiClient == null) return;

        try
        {
            var data = new Dictionary<string, object>();

            if (report.Status != BatteryStatus.NotPresent)
            {
                data["level"] = report.RemainingCapacityInMilliwattHours.HasValue ?
                    (report.RemainingCapacityInMilliwattHours.Value * 100.0 / report.FullChargeCapacityInMilliwattHours.Value) : 0;
                data["charging"] = report.Status == BatteryStatus.Charging;
                data["status"] = report.Status.ToString();
            }
            else
            {
                data["level"] = -1; // Unknown
                data["charging"] = false;
                data["status"] = "not_present";
            }

            await Node.Instance.quixiClient.SendCommandAsync("sensor.battery", "update", data);
        }
        catch (Exception ex)
        {
            Logging.error($"Failed to send battery data: {ex.Message}");
        }
    }

    public Windows.Devices.Power.BatteryReport? GetCurrentBatteryReport()
    {
        try
        {
            return Windows.Devices.Power.Battery.AggregateBattery.GetReport();
        }
        catch (Exception ex)
        {
            Logging.error($"Failed to get battery report: {ex.Message}");
            return null;
        }
    }
}