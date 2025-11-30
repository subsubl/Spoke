using IXICore;
using IXICore.Meta;
using IXICore.Network;
using IxiHome.Network;

namespace Spoke.Meta;

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
    public WebSocketManager? webSocketManager { get; private set; }
    
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

                // Initialize WebSocket manager for real-time updates
                webSocketManager = WebSocketManager.Instance;
                Logging.info("WebSocket manager initialized");
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
    public async Task shutdownAsync()
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

        // Disconnect WebSocket
        if (webSocketManager != null)
        {
            try
            {
                await webSocketManager.DisconnectAsync();
                Logging.info("WebSocket disconnected");
            }
            catch (Exception ex)
            {
                Logging.error($"Error disconnecting WebSocket: {ex.Message}");
            }
        }

        initialized = false;
        Logging.info("IxiHome Node shutdown complete");
    }

    /// <summary>
    /// Start the WebSocket connection for real-time updates
    /// </summary>
    public async Task StartWebSocketAsync()
    {
        if (webSocketManager != null && !string.IsNullOrEmpty(Config.quixiAddress))
        {
            try
            {
                await webSocketManager.ConnectAsync();
                Logging.info("WebSocket connection started");
            }
            catch (Exception ex)
            {
                Logging.error($"Failed to start WebSocket connection: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Reconnect to QuIXI with updated configuration
    /// </summary>
    public async Task ReconnectQuixiAsync()
    {
        if (quixiClient != null)
        {
            quixiClient.Disconnect();
            quixiClient = null;
        }

        if (webSocketManager != null)
        {
            await webSocketManager.DisconnectAsync();
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

                // Reconnect WebSocket
                if (webSocketManager != null)
                {
                    await webSocketManager.ReconnectAsync();
                    Logging.info("WebSocket reconnected");
                }
            }
            catch (Exception ex)
            {
                Logging.error($"Failed to reconnect QuIXI client: {ex.Message}");
            }
        }
    }
}


