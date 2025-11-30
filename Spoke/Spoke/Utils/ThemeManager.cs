namespace Spoke.Utils;

/// <summary>
/// Manages application theme (light/dark mode)
/// </summary>
public static class ThemeManager
{
    /// <summary>
    /// Sets the application theme
    /// </summary>
    public static void SetTheme(bool useDarkTheme)
    {
        if (Application.Current == null)
            return;

        Application.Current.UserAppTheme = useDarkTheme ? AppTheme.Dark : AppTheme.Light;
    }

    /// <summary>
    /// Gets the current theme
    /// </summary>
    public static bool IsDarkTheme()
    {
        if (Application.Current == null)
            return false;

        return Application.Current.UserAppTheme == AppTheme.Dark ||
               (Application.Current.UserAppTheme == AppTheme.Unspecified &&
                Application.Current.RequestedTheme == AppTheme.Dark);
    }

    /// <summary>
    /// Toggles between light and dark theme
    /// </summary>
    public static void ToggleTheme()
    {
        SetTheme(!IsDarkTheme());
    }
}


