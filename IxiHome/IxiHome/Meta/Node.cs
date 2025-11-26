using IXICore;
using IXICore.Meta;
using IXICore.Network;
using IxiHome.Network;

namespace IxiHome.Meta;

public class Node
{
    private static Node? _instance = null;
    public static Node Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Node();
            }
            return _instance;
        }
    }

    public QuixiClient? quixiClient { get; private set; }
    
    private bool initialized = false;

    private Node()
    {
        // Private constructor for singleton
    }

    /// <summary>
    /// Initialize the Ixian node and QuIXI client
    /// </summary>
    public void init()
    {
        if (initialized)
        {
            Logging.warn("Node already initialized");
            return;
        }

        Logging.info("Initializing IxiHome Node");

        // Load configuration
        Config.Load();

        // Initialize IXI Core configuration
        CoreConfig.productVersion = Config.version;
            // CoreConfig property not needed for IxiHome        // Initialize QuIXI client if configured
        if (!string.IsNullOrEmpty(Config.quixiAddress))
        {
            try
            {
                quixiClient = new QuixiClient(
                    Config.quixiAddress,
                    Config.quixiApiPort,
                    Config.quixiSecure,
                    Config.quixiUsername,
                    Config.quixiPassword
                );

                Logging.info($"QuIXI client initialized for {Config.quixiAddress}:{Config.quixiApiPort}");
            }
            catch (Exception ex)
            {
                Logging.error($"Failed to initialize QuIXI client: {ex.Message}");
            }
        }
        else
        {
            Logging.warn("QuIXI address not configured. Client not initialized.");
        }

        initialized = true;
        Logging.info("IxiHome Node initialization complete");
    }

    /// <summary>
    /// Shutdown the node and cleanup resources
    /// </summary>
    public void shutdown()
    {
        if (!initialized)
        {
            return;
        }

        Logging.info("Shutting down IxiHome Node");

        // Disconnect QuIXI client
        if (quixiClient != null)
        {
            try
            {
                quixiClient.Disconnect();
                Logging.info("QuIXI client disconnected");
            }
            catch (Exception ex)
            {
                Logging.error($"Error disconnecting QuIXI client: {ex.Message}");
            }
        }

        initialized = false;
        Logging.info("IxiHome Node shutdown complete");
    }

    /// <summary>
    /// Reconnect to QuIXI with updated configuration
    /// </summary>
    public void ReconnectQuixi()
    {
        if (quixiClient != null)
        {
            quixiClient.Disconnect();
            quixiClient = null;
        }

        if (!string.IsNullOrEmpty(Config.quixiAddress))
        {
            try
            {
                quixiClient = new QuixiClient(
                    Config.quixiAddress,
                    Config.quixiApiPort,
                    Config.quixiSecure,
                    Config.quixiUsername,
                    Config.quixiPassword
                );

                Logging.info($"QuIXI client reconnected to {Config.quixiAddress}:{Config.quixiApiPort}");
            }
            catch (Exception ex)
            {
                Logging.error($"Failed to reconnect QuIXI client: {ex.Message}");
            }
        }
    }
}
