using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System.Diagnostics;
using Windows.Graphics;

namespace IxiHome.WinUI;

public partial class App : Microsoft.Maui.MauiWinUIApplication
{
    const int WindowWidth = 1400;
    const int WindowHeight = 900;
    const int MinWidth = 800;
    const int MinHeight = 600;

    public App()
    {
        this.InitializeComponent();

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
