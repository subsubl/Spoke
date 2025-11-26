using CommunityToolkit.Maui;
using IXICore;
using IXICore.Meta;
using IXICore.Network;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace IxiHome;

public static class MauiProgram
{
    public static int ActiveActivityCount;
    private static CancellationTokenSource? _shutdownCts;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCompatibility()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android =>
                {
                    android.OnCreate((activity, bundle) =>
                    {
                        Interlocked.Increment(ref ActiveActivityCount);
                        Logging.info($"{activity.GetType().Name} created. Active count = {ActiveActivityCount}");
                        
                        // Cancel any pending shutdown if a new activity starts
                        _shutdownCts?.Cancel();
                    });

                    android.OnDestroy((activity) =>
                    {
                        var count = Math.Max(0, Interlocked.Decrement(ref ActiveActivityCount));
                        Logging.info($"{activity.GetType().Name} destroyed. Active count = {count}");

                        if (count <= 0 && !activity.IsChangingConfigurations)
                        {
                            Logging.info("Last activity destroyed - scheduling delayed shutdown");
                            
                            _shutdownCts = new CancellationTokenSource();
                            var token = _shutdownCts.Token;

                            Task.Run(async () =>
                            {
                                try
                                {
                                    await Task.Delay(1000, token); // 1 second debounce
                                    if (!token.IsCancellationRequested)
                                    {
                                        Logging.info("No new activity started - shutting down Node");
                                        await App.Shutdown();
                                    }
                                }
                                catch (TaskCanceledException)
                                {
                                    Logging.info("Shutdown cancelled - new activity started");
                                }
                            });
                        }
                    });

                    android.OnPause((activity) =>
                    {
                        App.isInForeground = false;
                        Logging.info($"{activity.GetType().Name} paused. isInForeground = false");
                    });

                    android.OnResume((activity) =>
                    {
                        App.isInForeground = true;
                        Logging.info($"{activity.GetType().Name} resumed. isInForeground = true");
                    });
                });
#endif
#if IOS || MACCATALYST
                events.AddiOS(ios =>
                {
                    ios.OnActivated((app) =>
                    {
                        App.isInForeground = true;
                        Logging.info("iOS app activated. isInForeground = true");
                    });

                    ios.OnResignActivation((app) =>
                    {
                        App.isInForeground = false;
                        Logging.info("iOS app resigned activation. isInForeground = false");
                    });
                });
#endif
#if WINDOWS
                events.AddWindows(windows =>
                {
                    windows.OnClosed(async (window, args) =>
                    {
                        Logging.info("Windows OnWindowClosed - shutting down Node");
                        await App.Shutdown();
                    });

                    windows.OnWindowCreated((window) =>
                    {
                        Logging.info("Windows OnWindowCreated - ensuring Node is running");
                        App.isInForeground = true;
                        App.EnsureNodeRunning();
                    });

                    windows.OnActivated((window, args) =>
                    {
                        App.isInForeground = true;
                        Logging.info("Windows app activated. isInForeground = true");
                    });

                    windows.OnVisibilityChanged((window, args) =>
                    {
                        App.isInForeground = args.Visible;
                        Logging.info($"Windows app visibility changed. isInForeground = {args.Visible}");
                    });
                });
#endif
            });

        return builder.Build();
    }
}
