using Xunit;
using Spoke.Core;

namespace Spoke.Tests;

public class AppInitializationTests
{
    [Fact]
    public void AppConstructor_DoesNotStartNodeAutoInit_WhenQuixiConfigured()
    {
        // Arrange - ensure clean state (reset the guard) for tests
        StartupGuard.ResetForTests();

        // Initially the guard must be false
        Assert.False(StartupGuard.AutoInitStarted);

        // Act - call TryStartAutoInit (no initializer) and ensure it flips to true
        StartupGuard.TryStartAutoInit();

        // Assert
        Assert.True(StartupGuard.AutoInitStarted);

        // Reset and check initializer runs only once
        int counter = 0;
        StartupGuard.ResetForTests();
        StartupGuard.TryStartAutoInit(() => { System.Threading.Interlocked.Increment(ref counter); return System.Threading.Tasks.Task.CompletedTask; });
        // second attempt should not increment
        StartupGuard.TryStartAutoInit(() => { System.Threading.Interlocked.Increment(ref counter); return System.Threading.Tasks.Task.CompletedTask; });

        // wait up to 1s for background initializer to complete (avoid timing races)
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 1000 && counter == 0)
        {
            System.Threading.Thread.Sleep(10);
        }

        Assert.Equal(1, counter);
    }
}
