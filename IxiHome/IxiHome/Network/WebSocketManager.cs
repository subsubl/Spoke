using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IXICore;
using IXICore.Meta;
using IxiHome.Meta;
using IxiHome.Data;
using System.Text.Json;

namespace IxiHome.Network
{
    /// <summary>
    /// WebSocket manager for real-time updates from Ixian network
    /// Handles live entity state updates, events, and notifications
    /// </summary>
    public class WebSocketManager
    {
        private static WebSocketManager? _instance;
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _receiveTask;
        private bool _isConnected;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

        // Events for real-time updates
        public event EventHandler<EntityStateChangedEventArgs>? EntityStateChanged;
        public event EventHandler<NotificationReceivedEventArgs>? NotificationReceived;
        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        public static WebSocketManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WebSocketManager();
                }
                return _instance;
            }
        }

        public bool IsConnected => _isConnected;

        private WebSocketManager()
        {
            // Private constructor for singleton
        }

        /// <summary>
        /// Connect to the Ixian network websocket
        /// </summary>
        public async Task ConnectAsync()
        {
            await _connectionLock.WaitAsync();
            try
            {
                if (_isConnected)
                    return;

                _webSocket = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();

                string wsUrl = $"ws://{Config.quixiAddress}:{Config.quixiWebSocketPort}/ixi";

                try
                {
                    await _webSocket.ConnectAsync(new Uri(wsUrl), _cancellationTokenSource.Token);
                    _isConnected = true;

                    Logging.info("WebSocket connected to Ixian network");

                    // Start receiving messages
                    _receiveTask = ReceiveMessagesAsync();

                    // Send authentication message
                    await SendAuthenticationAsync();

                    ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(true));
                }
                catch (Exception ex)
                {
                    Logging.error($"WebSocket connection failed: {ex.Message}");
                    _isConnected = false;
                    ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(false));
                    throw;
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Disconnect from the websocket
        /// </summary>
        public async Task DisconnectAsync()
        {
            await _connectionLock.WaitAsync();
            try
            {
                if (!_isConnected)
                    return;

                _cancellationTokenSource?.Cancel();

                if (_webSocket != null)
                {
                    try
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);
                    }
                    catch
                    {
                        // Ignore close errors
                    }
                }

                _isConnected = false;
                ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(false));
                Logging.info("WebSocket disconnected");
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Send authentication message to the server
        /// </summary>
        private async Task SendAuthenticationAsync()
        {
            if (!_isConnected || _webSocket == null)
                return;

            var authMessage = new
            {
                type = "auth",
                username = Config.quixiUsername,
                password = Config.quixiPassword,
                client_type = "ixi_home",
                version = Config.version
            };

            string jsonMessage = JsonSerializer.Serialize(authMessage);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cancellationTokenSource?.Token ?? CancellationToken.None);
        }

        /// <summary>
        /// Subscribe to entity state changes
        /// </summary>
        public async Task SubscribeToEntitiesAsync()
        {
            if (!_isConnected || _webSocket == null)
                return;

            var subscribeMessage = new
            {
                type = "subscribe",
                subscription_type = "entities"
            };

            string jsonMessage = JsonSerializer.Serialize(subscribeMessage);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cancellationTokenSource?.Token ?? CancellationToken.None);
        }

        /// <summary>
        /// Subscribe to notifications
        /// </summary>
        public async Task SubscribeToNotificationsAsync()
        {
            if (!_isConnected || _webSocket == null)
                return;

            var subscribeMessage = new
            {
                type = "subscribe",
                subscription_type = "notifications"
            };

            string jsonMessage = JsonSerializer.Serialize(subscribeMessage);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cancellationTokenSource?.Token ?? CancellationToken.None);
        }

        /// <summary>
        /// Send an entity command
        /// </summary>
        public async Task SendEntityCommandAsync(string entityId, string command, object? parameters = null)
        {
            if (!_isConnected || _webSocket == null)
                return;

            var commandMessage = new
            {
                type = "entity_command",
                entity_id = entityId,
                command = command,
                parameters = parameters,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            string jsonMessage = JsonSerializer.Serialize(commandMessage);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cancellationTokenSource?.Token ?? CancellationToken.None);
        }

        /// <summary>
        /// Receive and process messages from the websocket
        /// </summary>
        private async Task ReceiveMessagesAsync()
        {
            if (_webSocket == null || _cancellationTokenSource == null)
                return;

            var buffer = new byte[4096];
            var messageBuffer = new StringBuilder();

            try
            {
                while (_isConnected && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                        string chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuffer.Append(chunk);
                    }
                    while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = messageBuffer.ToString();
                        messageBuffer.Clear();

                        await ProcessMessageAsync(message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Logging.info("WebSocket connection closed by server");
                        await DisconnectAsync();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.error($"WebSocket receive error: {ex.Message}");
                await DisconnectAsync();
            }
        }

        /// <summary>
        /// Process incoming websocket messages
        /// </summary>
        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(message);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("type", out var typeProperty))
                    return;

                string messageType = typeProperty.GetString() ?? "";

                switch (messageType)
                {
                    case "entity_state_changed":
                        await ProcessEntityStateChangedAsync(root);
                        break;

                    case "notification":
                        await ProcessNotificationAsync(root);
                        break;

                    case "auth_response":
                        await ProcessAuthResponseAsync(root);
                        break;

                    case "ping":
                        await SendPongAsync();
                        break;

                    default:
                        Logging.info($"Unknown message type: {messageType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logging.error($"Error processing websocket message: {ex.Message}");
            }
        }

        private async Task ProcessEntityStateChangedAsync(JsonElement message)
        {
            try
            {
                var entityId = message.GetProperty("entity_id").GetString() ?? "";
                var newState = message.GetProperty("state");

                // Update entity in our data manager
                var entity = EntityManager.Instance.GetEntityByEntityId(entityId);
                if (entity != null)
                {
                    entity.UpdateState(newState);
                    EntityStateChanged?.Invoke(this, new EntityStateChangedEventArgs(entity));
                }
            }
            catch (Exception ex)
            {
                Logging.error($"Error processing entity state change: {ex.Message}");
            }
        }

        private async Task ProcessNotificationAsync(JsonElement message)
        {
            try
            {
                var title = message.GetProperty("title").GetString() ?? "";
                var body = message.GetProperty("body").GetString() ?? "";
                var data = message.TryGetProperty("data", out var dataProp) ? dataProp : default;

                NotificationReceived?.Invoke(this, new NotificationReceivedEventArgs(title, body, data));
            }
            catch (Exception ex)
            {
                Logging.error($"Error processing notification: {ex.Message}");
            }
        }

        private async Task ProcessAuthResponseAsync(JsonElement message)
        {
            try
            {
                var success = message.GetProperty("success").GetBoolean();
                var messageText = message.GetProperty("message").GetString() ?? "";

                if (success)
                {
                    Logging.info("WebSocket authentication successful");
                    // Auto-subscribe to updates
                    await SubscribeToEntitiesAsync();
                    await SubscribeToNotificationsAsync();
                }
                else
                {
                    Logging.error($"WebSocket authentication failed: {messageText}");
                    await DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                Logging.error($"Error processing auth response: {ex.Message}");
            }
        }

        private async Task SendPongAsync()
        {
            if (!_isConnected || _webSocket == null)
                return;

            var pongMessage = new { type = "pong" };
            string jsonMessage = JsonSerializer.Serialize(pongMessage);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cancellationTokenSource?.Token ?? CancellationToken.None);
        }

        /// <summary>
        /// Reconnect to the websocket (used when settings change)
        /// </summary>
        public async Task ReconnectAsync()
        {
            await DisconnectAsync();
            await Task.Delay(1000); // Brief delay before reconnecting
            await ConnectAsync();
        }
    }

    // Event argument classes
    public class EntityStateChangedEventArgs : EventArgs
    {
        public Entity Entity { get; }

        public EntityStateChangedEventArgs(Entity entity)
        {
            Entity = entity;
        }
    }

    public class NotificationReceivedEventArgs : EventArgs
    {
        public string Title { get; }
        public string Body { get; }
        public JsonElement Data { get; }

        public NotificationReceivedEventArgs(string title, string body, JsonElement data)
        {
            Title = title;
            Body = body;
            Data = data;
        }
    }

    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        public bool IsConnected { get; }

        public ConnectionStatusChangedEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}