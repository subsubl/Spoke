using IXICore;
using IXICore.Meta;
using IXICore.Network;
using Spoke.Meta;
using System;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.IO;

namespace Spoke;

public partial class App : Application
{
    private static readonly string TraceLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "spoke_app_trace.log");
    private static void Trace(string message)
    {
        try
        {
            File.AppendAllText(TraceLogPath, $"[{DateTime.UtcNow:O}] {message}{Environment.NewLine}");
        }
        catch
        {
            // ignore trace failures
        }
    }

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
    // Guard is managed by Spoke.Core.StartupGuard so it can be tested without compiling XAML
    // Keep a bridge property for backwards compatibility and for any code referencing this value
    internal static bool NodeAutoInitStarted => global::Spoke.Core.StartupGuard.AutoInitStarted;

    public App()
    {
        Trace("App constructor start");
#if RUN_TESTS
        // Run tests instead of GUI app
        Program.Main(Array.Empty<string>()).Wait();
        Environment.Exit(0);
#else
        try
        {
            InitializeComponent();
            Trace("InitializeComponent succeeded");
        }
        catch (Exception ex)
        {
            try
            {
                File.AppendAllText(Path.Combine(Path.GetTempPath(), "spoke_xaml_diag.txt"), $"InitializeComponent failed: {ex.Message}\n{ex.StackTrace}\n");
            }
            catch { }
            throw;
        }

        // Prepare the personal folder
        try
        {
            var tempDiag = Path.Combine(Path.GetTempPath(), "spoke_temp_diag.txt");
            File.AppendAllText(tempDiag, $"App constructor start: {DateTime.UtcNow:O}{Environment.NewLine}");
            File.AppendAllText(tempDiag, $"spokeUserFolder: {Config.spokeUserFolder}{Environment.NewLine}");
        }
        catch { }
        if (!Directory.Exists(Config.spokeUserFolder))
        {
            Directory.CreateDirectory(Config.spokeUserFolder);
        }

        // Write a small diagnostic file to verify we can write into the user folder
        try
        {
            var diagPath = Path.Combine(Config.spokeUserFolder, "startup_diag.txt");
            File.AppendAllText(diagPath, $"Startup attempt: {DateTime.UtcNow:O}{Environment.NewLine}");
        }
        catch (Exception writeEx)
        {
            try
            {
                var tmp = Path.Combine(Path.GetTempPath(), "spoke_startup_diag.txt");
                File.AppendAllText(tmp, $"Diag write failed: {writeEx}{Environment.NewLine}");
            }
            catch { }
        }

        // Init logging (wrapped to capture failures to disk instead of exiting immediately)
        Logging.setOptions(Config.maxLogSize, Config.maxLogCount, false);
        bool loggingStarted = true;
        try
        {
            loggingStarted = Logging.start(Config.spokeUserFolder, Config.logVerbosity);
        }
        catch (Exception ex)
        {
            loggingStarted = false;
            try { File.AppendAllText(Path.Combine(Config.spokeUserFolder, "startup_diag.txt"), $"Logging.start threw: {ex}{Environment.NewLine}"); } catch { }
        }

        if (!loggingStarted)
        {
            try { File.AppendAllText(Path.Combine(Config.spokeUserFolder, "startup_diag.txt"), "Logging.start returned false\n"); } catch { }
            // Do not exit immediately — keep going so we can capture more diagnostics during startup.
        }
        Logging.info("Starting Spoke {0} ({1})", Config.version, CoreConfig.version);
        Logging.info("Operating System is {0}", IXICore.Platform.getOSNameAndVersion());

        // Init fatal exception handlers
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;
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

        // NOTE: Do not auto-initialize the node here - CreateWindow runs later and is a safer place to
        // start background services that may interact with UI or cause early activation artifacts on
        // some Windows configurations. Initialization is now performed inside CreateWindow.
        if (string.IsNullOrEmpty(Config.quixiAddress))
        {
            Logging.info("QuIXI not configured yet, node initialization deferred until settings are saved");
        }

        // Note: Node.init() will be called manually after QuIXI settings are configured in GUI
#endif

        Trace("Finished platform init");
        // Temporary workaround for .NET 9: set MainPage even though obsolete
        if (!Config.IsSetupComplete())
        {
            Trace("Setting MainPage to Onboarding");
            MainPage = new Pages.Onboarding.OnboardingPage();
        }
        else
        {
            Trace("Setting MainPage to AppShell");
            MainPage = new AppShell();
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Trace("CreateWindow invoked");
        try
        {
            var window = base.CreateWindow(activationState);

            window.Title = "Spoke";


            if (appWindow == null)
            {
                appWindow = window;
                Trace("appWindow set in CreateWindow");

                // If QuIXI is already configured in the saved config, start node initialization now
                // (safer to do it after a window exists to avoid platform activation/creation race issues)
                try
                {
                    if (!string.IsNullOrEmpty(Config.quixiAddress))
                    {
                        Logging.info("QuIXI address configured, attempting guarded auto-initialization after window created");
                        // delegate the single-run semantics and execution to StartupGuard
                        global::Spoke.Core.StartupGuard.TryStartAutoInit(async () => await InitializeNodeAsync());
                    }
                }
                catch (Exception ex)
                {
                    Logging.warn($"Auto-init after CreateWindow failed: {ex.Message}");
                }
            }

            return window;
        }
        catch (Exception ex)
        {
            try
            {
                File.AppendAllText(Path.Combine(Config.spokeUserFolder, "startup_diag.txt"), $"CreateWindow failed: {ex.Message}\n{ex.StackTrace}\n");
            }
            catch { }
            throw;
        }
    }
    
    /// <summary>
    /// Initialize Node and connect to QuIXI. Should be called after settings are configured.
    /// </summary>
    public static async Task InitializeNodeAsync()
    {
        try
        {
            Logging.info("Initializing Node and QuIXI connection");
            try
            {
                File.AppendAllText(Path.Combine(Config.spokeUserFolder, "startup_diag.txt"), $"Node init started: {DateTime.UtcNow:O}{Environment.NewLine}");
            }
            catch { }
            Node.Instance.init();
            
            // Start WebSocket connection for real-time updates
            await Node.Instance.StartWebSocketAsync();
            
            // Start sync service
            _ = Services.SyncService.Instance.StartAsync();

            // Initialize notifications
#if WINDOWS
            Notifications.NotificationManager.Instance.NotificationsEnabled = Config.enableNotifications;
#endif

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
        Trace($"TaskScheduler unobserved exception: {e.Exception}\n{e.Exception.StackTrace}");
        e.SetObserved();
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        Logging.error($"CurrentDomain Unhandled Exception: {exception}");
        Trace($"CurrentDomain unhandled exception: {exception}\n{exception?.StackTrace}");
    }

    private static void CurrentDomainOnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
    {
        Logging.warn($"FirstChanceException: {e.Exception?.GetType()}: {e.Exception?.Message}");
        Trace($"FirstChance exception: {e.Exception}\n{e.Exception?.StackTrace}");
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
        Logging.info("Spoke shutting down...");
        
        // Stop sync service
        await Services.SyncService.Instance.StopAsync();
        
        if (Node.Instance != null)
        {
            await Node.Instance.shutdownAsync();
        }

        await Task.Delay(500);
        Logging.info("Spoke shutdown complete");
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
