using IXICore;
using IXICore.Meta;
using IXICore.Network;
using IXICore.RegNames;
using Spoke.Network;

namespace Spoke.Meta;

internal class Node : IxianNode
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
    private bool running = false;

    // Wallet-related properties
    public static bool generatedNewWallet = false;
    public static string walletFile = "wallet.ixi";

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

        Logging.info("Initializing Spoke Node");

        // Load configuration
        Config.Load();

        // Initialize IXI Core configuration
        CoreConfig.productVersion = Config.version;

        // Initialize IxianHandler with basic network type
        IxianHandler.init(Config.version, this, NetworkType.main, false);

        // Initialize QuIXI client if configured
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
        Logging.info("Spoke Node initialization complete");
    }

    /// <summary>
    /// Checks for existing wallet file
    /// </summary>
    public static bool checkForExistingWallet()
    {
        string walletPath = Path.Combine(Config.spokeUserFolder, walletFile);
        if (!File.Exists(walletPath))
        {
            Logging.log(LogSeverity.error, "Cannot read wallet file.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Load existing wallet from storage
    /// </summary>
    public static bool loadWallet()
    {
        if (!Preferences.Default.ContainsKey("walletpass"))
        {
            return false;
        }

        // Get password from secure storage
        string password = Preferences.Default.Get("walletpass", "");

        string walletPath = Path.Combine(Config.spokeUserFolder, walletFile);
        WalletStorage walletStorage = new WalletStorage(walletPath);
        
        if (walletStorage.readWallet(password))
        {
            IxianHandler.addWallet(walletStorage);
            Logging.info("Wallet loaded successfully from {0}", walletPath);
            return true;
        }
        
        Logging.error("Failed to load wallet from {0}", walletPath);
        return false;
    }

    /// <summary>
    /// Generate a new wallet with the specified password
    /// </summary>
    public static bool generateWallet(string pass)
    {
        if (IxianHandler.getWalletList().Count == 0)
        {
            string walletPath = Path.Combine(Config.spokeUserFolder, walletFile);
            
            // Ensure the directory exists
            string? directory = Path.GetDirectoryName(walletPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            WalletStorage ws = new WalletStorage(walletPath);
            if (ws.generateWallet(pass))
            {
                bool added = IxianHandler.addWallet(ws);
                if (added)
                {
                    // Save the wallet password to preferences
                    Preferences.Default.Set("walletpass", pass);
                    
                    Logging.info("Wallet generated and saved successfully to {0}", walletPath);
                    return true;
                }
            }
            
            Logging.error("Failed to generate wallet");
        }
        else
        {
            Logging.warn("Wallet already exists, cannot generate new wallet");
        }
        return false;
    }

    /// <summary>
    /// Start the node and its services
    /// </summary>
    public static void start()
    {
        if (Instance.running)
        {
            Logging.warn("Node already running");
            return;
        }

        Logging.info("Starting Spoke Node");
        Instance.running = true;

        // Check if wallet exists and load it
        if (generatedNewWallet || !checkForExistingWallet())
        {
            generatedNewWallet = false;
        }

        // Additional startup logic can be added here
        // For now, Spoke is primarily a QuIXI client without full blockchain functionality

        Logging.info("Spoke Node started successfully");
    }

    /// <summary>
    /// Stop the node
    /// </summary>
    public static void stop()
    {
        if (!Instance.running)
        {
            return;
        }

        Logging.info("Stopping Spoke Node");
        Instance.running = false;
        
        Logging.info("Spoke Node stopped");
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

        Logging.info("Shutting down Spoke Node");

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
        Logging.info("Spoke Node shutdown complete");
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

    // Implementation of IxianNode abstract methods
    // Note: Spoke is a lightweight client and doesn't maintain full blockchain functionality
    // These methods return minimal/stub implementations

    public override ulong getHighestKnownNetworkBlockHeight()
    {
        // Spoke doesn't track network block height - it's a QuIXI client
        return 0;
    }

    public override Block getBlockHeader(ulong blockNum)
    {
        // Spoke doesn't store block headers
        return null;
    }

    public override byte[] getBlockHash(ulong blockNum)
    {
        // Spoke doesn't store block hashes
        return null;
    }

    public override Block getLastBlock()
    {
        // Spoke doesn't track blocks
        return null;
    }

    public override ulong getLastBlockHeight()
    {
        // Spoke doesn't track block height
        return 0;
    }

    public override int getLastBlockVersion()
    {
        // Return default block version
        return 0;
    }

    public override bool addTransaction(Transaction tx, List<Address> relayNodeAddresses, bool force_broadcast)
    {
        // Spoke doesn't handle transactions directly
        // This would need to be implemented if Spoke needs to send transactions
        Logging.warn("Transaction handling not implemented in Spoke");
        return false;
    }

    public override bool isAcceptingConnections()
    {
        // Spoke is a client, not a server
        return false;
    }

    public override Wallet getWallet(Address id)
    {
        // Spoke doesn't maintain wallet balances - QuIXI handles this
        return null;
    }

    public override IxiNumber getWalletBalance(Address id)
    {
        // Spoke doesn't track balances - QuIXI handles this
        return 0;
    }

    public override void parseProtocolMessage(ProtocolMessageCode code, byte[] data, RemoteEndpoint endpoint)
    {
        // Spoke doesn't handle protocol messages - it's a QuIXI client
        Logging.warn("Protocol message handling not implemented in Spoke");
    }

    public override void shutdown()
    {
        // Synchronous shutdown
        stop();
        shutdownAsync().Wait();
    }

    public override IxiNumber getMinSignerPowDifficulty(ulong blockNum, int curBlockVersion, long curBlockTimestamp)
    {
        // Spoke doesn't use PoW
        return 0;
    }

    public override RegisteredNameRecord getRegName(byte[] name, bool useAbsoluteId)
    {
        // Spoke doesn't handle registered names
        return null;
    }
}




