namespace IxiHome.Meta;

public static class Config
{
    // Application version
    public static readonly string version = "0.1.0";

    // User folder paths
    private static string appFolderPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal), "IxiHome");

    public static string ixiHomeUserFolder
    {
        get => appFolderPath;
        set => appFolderPath = value;
    }

    // Logging settings
    public static int maxLogSize = 50; // MB
    public static int maxLogCount = 5; // number of log files to keep
    public static int logVerbosity = 2; // 0 = minimal, 1 = normal, 2 = verbose, 3+ = trace

    // QuIXI connection settings
    public static string quixiAddress = "";
    public static int quixiApiPort = 8001;
    public static bool quixiSecure = false;
    public static string quixiUsername = "";
    public static string quixiPassword = "";

    // Home Assistant settings
    public static string homeAssistantUrl = "";
    public static string homeAssistantToken = "";

    // MQTT settings (if connecting directly)
    public static string mqttBroker = "localhost";
    public static int mqttPort = 1883;
    public static string mqttUsername = "";
    public static string mqttPassword = "";
    public static string mqttTopicPrefix = "homeassistant";

    // App settings
    public static bool useDarkTheme = false;
    public static bool enableNotifications = true;
    public static int entityRefreshInterval = 5000; // milliseconds
    public static bool requireAuthentication = false;
    public static string languageCode = "en";

    // Entity storage
    public static string entitiesFileName = "entities.json";
    public static string entitiesFilePath => System.IO.Path.Combine(ixiHomeUserFolder, entitiesFileName);

    /// <summary>
    /// Loads configuration from Preferences
    /// </summary>
    public static void Load()
    {
        quixiAddress = Preferences.Default.Get(nameof(quixiAddress), quixiAddress);
        quixiApiPort = Preferences.Default.Get(nameof(quixiApiPort), quixiApiPort);
        quixiSecure = Preferences.Default.Get(nameof(quixiSecure), quixiSecure);
        quixiUsername = Preferences.Default.Get(nameof(quixiUsername), quixiUsername);

        homeAssistantUrl = Preferences.Default.Get(nameof(homeAssistantUrl), homeAssistantUrl);
        
        mqttBroker = Preferences.Default.Get(nameof(mqttBroker), mqttBroker);
        mqttPort = Preferences.Default.Get(nameof(mqttPort), mqttPort);
        mqttUsername = Preferences.Default.Get(nameof(mqttUsername), mqttUsername);
        mqttTopicPrefix = Preferences.Default.Get(nameof(mqttTopicPrefix), mqttTopicPrefix);

        useDarkTheme = Preferences.Default.Get(nameof(useDarkTheme), useDarkTheme);
        enableNotifications = Preferences.Default.Get(nameof(enableNotifications), enableNotifications);
        entityRefreshInterval = Preferences.Default.Get(nameof(entityRefreshInterval), entityRefreshInterval);
        requireAuthentication = Preferences.Default.Get(nameof(requireAuthentication), requireAuthentication);
        languageCode = Preferences.Default.Get(nameof(languageCode), languageCode);

        // Load sensitive data from SecureStorage
        try
        {
            var savedQuixiPassword = SecureStorage.Default.GetAsync(nameof(quixiPassword)).Result;
            if (savedQuixiPassword != null)
                quixiPassword = savedQuixiPassword;

            var savedHAToken = SecureStorage.Default.GetAsync(nameof(homeAssistantToken)).Result;
            if (savedHAToken != null)
                homeAssistantToken = savedHAToken;

            var savedMqttPassword = SecureStorage.Default.GetAsync(nameof(mqttPassword)).Result;
            if (savedMqttPassword != null)
                mqttPassword = savedMqttPassword;
        }
        catch
        {
            // SecureStorage might not be available in some scenarios
        }
    }

    /// <summary>
    /// Saves configuration to Preferences
    /// </summary>
    public static void Save()
    {
        Preferences.Default.Set(nameof(quixiAddress), quixiAddress);
        Preferences.Default.Set(nameof(quixiApiPort), quixiApiPort);
        Preferences.Default.Set(nameof(quixiSecure), quixiSecure);
        Preferences.Default.Set(nameof(quixiUsername), quixiUsername);

        Preferences.Default.Set(nameof(homeAssistantUrl), homeAssistantUrl);
        
        Preferences.Default.Set(nameof(mqttBroker), mqttBroker);
        Preferences.Default.Set(nameof(mqttPort), mqttPort);
        Preferences.Default.Set(nameof(mqttUsername), mqttUsername);
        Preferences.Default.Set(nameof(mqttTopicPrefix), mqttTopicPrefix);

        Preferences.Default.Set(nameof(useDarkTheme), useDarkTheme);
        Preferences.Default.Set(nameof(enableNotifications), enableNotifications);
        Preferences.Default.Set(nameof(entityRefreshInterval), entityRefreshInterval);
        Preferences.Default.Set(nameof(requireAuthentication), requireAuthentication);
        Preferences.Default.Set(nameof(languageCode), languageCode);

        // Save sensitive data to SecureStorage
        try
        {
            if (!string.IsNullOrEmpty(quixiPassword))
                SecureStorage.Default.SetAsync(nameof(quixiPassword), quixiPassword);

            if (!string.IsNullOrEmpty(homeAssistantToken))
                SecureStorage.Default.SetAsync(nameof(homeAssistantToken), homeAssistantToken);

            if (!string.IsNullOrEmpty(mqttPassword))
                SecureStorage.Default.SetAsync(nameof(mqttPassword), mqttPassword);
        }
        catch
        {
            // SecureStorage might not be available in some scenarios
        }
    }

    /// <summary>
    /// Checks if initial setup has been completed
    /// </summary>
    public static bool IsSetupComplete()
    {
        return Preferences.Default.Get("setup_complete", false);
    }

    /// <summary>
    /// Marks initial setup as complete
    /// </summary>
    public static void SetSetupComplete(bool complete = true)
    {
        Preferences.Default.Set("setup_complete", complete);
    }
}
