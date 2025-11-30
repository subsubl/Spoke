using Microsoft.Maui.Storage;

Console.WriteLine("Resetting IxiHome setup preferences...");

// Reset setup complete flag
Preferences.Default.Set("setup_complete", false);

// Clear QuIXI settings
Preferences.Default.Remove("quixiAddress");
Preferences.Default.Remove("quixiApiPort");
Preferences.Default.Remove("quixiWebSocketPort");
Preferences.Default.Remove("quixiSecure");
Preferences.Default.Remove("quixiUsername");

// Clear Home Assistant settings
Preferences.Default.Remove("homeAssistantUrl");

// Clear other settings to start fresh
Preferences.Default.Remove("useDarkTheme");
Preferences.Default.Remove("enableNotifications");
Preferences.Default.Remove("entityRefreshInterval");
Preferences.Default.Remove("requireAuthentication");
Preferences.Default.Remove("languageCode");

Console.WriteLine("Preferences reset. Onboarding will show on next app launch.");