using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Spoke.Meta;
using System.Diagnostics;
using Windows.Graphics;

namespace Spoke.WinUI;

public partial class App : MauiWinUIApplication
{
    const int WindowWidth = 1600;
    const int WindowHeight = 1200;
    const int MinWidth = 600;
    const int MinHeight = 840;

    public App()
    {
        // Ensure single-instance behavior for desktop app
        var singleInstance = AppInstance.FindOrRegisterForKey("SpokeDesktopApp");
        if (!singleInstance.IsCurrent)
        {
            var currentInstance = AppInstance.GetCurrent();
            var args = currentInstance.GetActivatedEventArgs();
            singleInstance.RedirectActivationToAsync(args).GetAwaiter().GetResult();

            Process.GetCurrentProcess().Kill();
            return;
        }

        singleInstance.Activated += OnAppInstanceActivated;

        InitializeComponent();

        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
            var nativeWindow = handler.PlatformView;
            nativeWindow.Activate();

            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(WindowWidth, WindowHeight));

            appWindow.Changed += (sender, args) =>
            {
                if (appWindow.Size.Width < MinWidth || appWindow.Size.Height < MinHeight)
                {
                    var newSize = new SizeInt32
                    {
                        Width = Math.Max(appWindow.Size.Width, MinWidth),
                        Height = Math.Max(appWindow.Size.Height, MinHeight)
                    };
                    appWindow.Resize(newSize);
                }
            };
        });

        // copy any embedded resources we need (HTML, samples, etc.) into the user's app folder
        CopyHtmlResources();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
    }

    private void OnAppInstanceActivated(object? sender, AppActivationArguments e)
    {
        Services.GetRequiredService<ILifecycleEventService>().OnAppInstanceActivated(sender, e);
    }

    private void CopyHtmlResources()
    {
        try
        {
            string sourceDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "html");
            string targetDirectory = Path.Combine(Config.spokeUserFolder, "html");
            CopyContents(sourceDirectory, targetDirectory);
        }
        catch { }
    }

    private void CopyContents(string sourceDirectory, string targetDirectory)
    {
        try
        {
            Directory.CreateDirectory(targetDirectory);

            foreach (string file in Directory.GetFiles(sourceDirectory))
            {
                string destFile = Path.Combine(targetDirectory, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subdir in Directory.GetDirectories(sourceDirectory))
            {
                string destSubdir = Path.Combine(targetDirectory, Path.GetFileName(subdir));
                CopyContents(subdir, destSubdir);
            }
        }
        catch { }
    }
}
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System.Diagnostics;
using System.IO;
using Windows.Graphics;

namespace Spoke.WinUI;

public partial class App : Microsoft.Maui.MauiWinUIApplication
{
    const int WindowWidth = 1400;
    const int WindowHeight = 900;
    const int MinWidth = 800;
    const int MinHeight = 600;

    public App()
    {
        try
        {
            this.InitializeComponent();
        }
        catch (Exception ex)
        {
            try
            {
                var diagPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "spoke_win_diag.txt");
                File.AppendAllText(diagPath, $"Windows App InitializeComponent failed: {ex}{Environment.NewLine}");
            }
            catch { }
            throw;
        }

        // Configure window properties
        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
            var mauiWindow = handler.VirtualView;
            var nativeWindow = handler.PlatformView;
            nativeWindow.Activate();
            
            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            
            // Set initial size
            appWindow.Resize(new SizeInt32(WindowWidth, WindowHeight));

            // Enforce minimum size
            appWindow.Changed += (sender, args) =>
            {
                if (appWindow.Size.Width < MinWidth || appWindow.Size.Height < MinHeight)
                {
                    var newSize = new SizeInt32
                    {
                        Width = Math.Max(appWindow.Size.Width, MinWidth),
                        Height = Math.Max(appWindow.Size.Height, MinHeight)
                    };
                    appWindow.Resize(newSize);
                }
            };
        });
    }

    protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
    }
}


