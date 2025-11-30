using IXICore;
using IXICore.Meta;
using IXICore.Network;
using IxiHome.Meta;
using System;
using System.Globalization;

namespace Spoke;

public partial class App : Application
{
    private static App? _singletonInstance;

    public static App Instance()
    {
        if (_singletonInstance == null)
        {
            _singletonInstance = new App();
        }
        return _singletonInstance;
    }

    public static bool isInForeground { get; set; } = false;
    public static Window? appWindow { get; private set; } = null;

    public App()
    {
#if RUN_TESTS
        // Run tests instead of GUI app
        Program.Main(Array.Empty<string>()).Wait();
        Environment.Exit(0);
#else
        InitializeComponent();

        // Prepare the personal folder
        if (!Directory.Exists(Config.ixiHomeUserFolder))
        {
            Directory.CreateDirectory(Config.ixiHomeUserFolder);
        }

        // Init logging
        Logging.setOptions(Config.maxLogSize, Config.maxLogCount, false);
        if (!Logging.start(Config.ixiHomeUserFolder, Config.logVerbosity))
        {
            Environment.Exit(1);
            return;
        }
        Logging.info("Starting IxiHome {0} ({1})", Config.version, CoreConfig.version);
        Logging.info("Operating System is {0}", IXICore.Platform.getOSNameAndVersion());

        // Init fatal exception handlers
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

        // Generate or load a device ID
        bool generate_uid = true;

        if (Preferences.Default.ContainsKey("uid"))
        {
            try
            {
                string? uid = Preferences.Default.Get("uid", string.Empty);
                if (!string.IsNullOrEmpty(uid))
                {
                    CoreConfig.device_id = Convert.FromBase64String(uid);
                    generate_uid = false;
                }
            }
            catch
            {
                Logging.warn("Corrupted uid value in preferences.");
            }
        }

        if (generate_uid)
        {
            CoreConfig.device_id = Guid.NewGuid().ToByteArray();
            Preferences.Default.Set("uid", Convert.ToBase64String(CoreConfig.device_id));
        }

        // Load configuration
        Config.Load();

        // Auto-initialize if QuIXI is configured
        if (!string.IsNullOrEmpty(Config.quixiAddress))
        {
            Logging.info("QuIXI address configured, auto-initializing node");
            _ = Task.Run(async () => await InitializeNodeAsync());
        }
        else
        {
            Logging.info("QuIXI not configured yet, node initialization deferred until settings are saved");
        }

        // Note: Node.init() will be called manually after QuIXI settings are configured in GUI
#endif
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        if (MainPage == null)
        {
            if (!Config.IsSetupComplete())
            {
                MainPage = new Pages.Onboarding.OnboardingPage();
            }
            else
            {
                MainPage = new AppShell();
            }
        }

        var window = base.CreateWindow(activationState);
        window.Title = "IxiHome";

        if (appWindow == null)
        {
            appWindow = window;
        }

        return window;
    }
    
    /// <summary>
    /// Initialize Node and connect to QuIXI. Should be called after settings are configured.
    /// </summary>
    public static async Task InitializeNodeAsync()
    {
        try
        {
            Logging.info("Initializing Node and QuIXI connection");
            Node.Instance.init();
            
            // Start WebSocket connection for real-time updates
            await Node.Instance.StartWebSocketAsync();
            
            // Start sync service
            _ = Services.SyncService.Instance.StartAsync();

            // Initialize notifications
            Notifications.NotificationManager.Instance.NotificationsEnabled = Config.enableNotifications;

            // Start sensor collection
            Sensors.SensorManager.Instance.StartAllSensors();

            // Start automation processing
            Data.AutomationManager.Instance.Start();

            // Start widget manager
            Widgets.WidgetManager.Instance.Start();

            // Create sample scenes and automations for testing
            CreateSampleScenesAndAutomations();
        }
        catch (Exception ex)
        {
            Logging.error($"Error initializing Node: {ex.Message}");
        }
    }

    private static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Logging.error($"TaskScheduler Unobserved Task Exception: {e.Exception}");
        e.SetObserved();
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        Logging.error($"CurrentDomain Unhandled Exception: {exception}");
    }

    public static void EnsureNodeRunning()
    {
        try
        {
            if (Node.Instance == null)
            {
                Logging.info("EnsureNodeRunning: Node.Instance is null - will be initialized in App constructor");
                // Node will be created during App initialization
            }
            else if (IxianHandler.status == NodeStatus.stopped)
            {
                Logging.info("EnsureNodeRunning: Node exists but is stopped");
                // TODO: restart if needed
            }
            else
            {
                Logging.info("EnsureNodeRunning: Node is already running");
            }
        }
        catch (Exception ex)
        {
            Logging.error($"EnsureNodeRunning exception: {ex}");
        }
    }

    public static async Task Shutdown()
    {
        Logging.info("IxiHome shutting down...");
        
        // Stop sync service
        await Services.SyncService.Instance.StopAsync();
        
        if (Node.Instance != null)
        {
            await Node.Instance.shutdownAsync();
        }

        await Task.Delay(500);
        Logging.info("IxiHome shutdown complete");
        Logging.flush();
        Logging.stop();
    }

    private static void CreateSampleScenesAndAutomations()
    {
        try
        {
            // Create a sample scene
            var movieScene = new Data.Scene
            {
                Id = "sample_movie_scene",
                Name = "Movie Night",
                Icon = "mdi:movie"
            };

            // Add some sample entities (these would normally come from Home Assistant)
            movieScene.AddEntityState("light.living_room", "off");
            movieScene.AddEntityState("light.kitchen", "off");
            movieScene.AddEntityState("switch.tv", "on");

            Data.SceneManager.Instance.AddOrUpdateScene(movieScene);

            // Create a sample automation
            var motionLightAutomation = Data.AutomationManager.Instance.CreateSimpleAutomation(
                "Motion Light",
                "binary_sensor.motion_living_room", // trigger entity
                "on", // trigger state
                "light.living_room", // action entity
                "on" // action state
            );

            Logging.info("Sample scenes and automations created");
        }
        catch (Exception ex)
        {
            Logging.error($"Error creating sample scenes and automations: {ex.Message}");
        }
    }
}


