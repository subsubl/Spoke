namespace Spoke.Core;

public static class StartupGuard
{
    // Guard indicating whether the app has already started auto-init.
    private static bool _autoInitStarted = false;

    // Expose read-only status for tests and diagnostics
    public static bool AutoInitStarted => _autoInitStarted;

    // Try to start auto initialization once. This method is thread-safe.
    // 'initializer' is called the first time this is invoked and only once.
    public static void TryStartAutoInit(Func<System.Threading.Tasks.Task>? initializer = null)
    {
        lock (typeof(StartupGuard))
        {
            if (_autoInitStarted) return;
            _autoInitStarted = true;
        }

        // Run initializer in background if provided
        if (initializer != null)
        {
            _ = System.Threading.Tasks.Task.Run(initializer);
        }
    }

    // Reset for tests.
    public static void ResetForTests()
    {
        lock (typeof(StartupGuard)) { _autoInitStarted = false; }
    }
}
