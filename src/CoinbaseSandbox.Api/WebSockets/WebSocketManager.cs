using System.Text.Json.Serialization;

namespace CoinbaseSandbox.Api.WebSockets;

using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CoinbaseSandbox.Domain.Models;
using CoinbaseSandbox.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages WebSocket connections for real-time data feeds
/// </summary>
public class WebSocketManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebSocketManager> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Timer _heartbeatTimer;

    // Track subscriptions for each websocket connection
    private readonly ConcurrentDictionary<string, HashSet<string>> _subscriptions = new();

    // Track last known prices for each product to detect changes
    private readonly ConcurrentDictionary<string, decimal> _lastPrices = new();

    public WebSocketManager(IServiceProvider serviceProvider, ILogger<WebSocketManager> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Set up a heartbeat timer (every 30 seconds)
        _heartbeatTimer = new Timer(SendHeartbeats, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

        // Start the price update monitoring
        _ = MonitorPriceUpdatesAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Adds a new WebSocket connection
    /// </summary>
    public void AddSocket(string socketId, WebSocket socket)
    {
        _sockets.TryAdd(socketId, socket);
        _subscriptions.TryAdd(socketId, new HashSet<string>());

        _logger.LogInformation("WebSocket added: {SocketId}", socketId);
    }

    /// <summary>
    /// Removes a WebSocket connection
    /// </summary>
    public void RemoveSocket(string socketId)
    {
        if (_sockets.TryRemove(socketId, out _))
        {
            _subscriptions.TryRemove(socketId, out _);
            _logger.LogInformation("WebSocket removed: {SocketId}", socketId);
        }
    }

    /// <summary>
    /// Processes a subscription message from a WebSocket client
    /// </summary>
    public async Task ProcessMessageAsync(string socketId, string message)
    {
        try
        {
            var request = JsonSerializer.Deserialize<SubscriptionRequest>(message);

            if (request == null)
            {
                await SendErrorAsync(socketId, "Invalid request format");
                return;
            }

            // Handle different types of requests
            switch (request.Type.ToLower())
            {
                case "subscribe":
                    await HandleSubscribeAsync(socketId, request);
                    break;

                case "unsubscribe":
                    await HandleUnsubscribeAsync(socketId, request);
                    break;

                default:
                    await SendErrorAsync(socketId, $"Unknown request type: {request.Type}");
                    break;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error processing WebSocket message: {Message}", message);
            await SendErrorAsync(socketId, "Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WebSocket message: {Message}", message);
            await SendErrorAsync(socketId, "Internal server error");
        }
    }

    /// <summary>
    /// Handles a subscribe request
    /// </summary>
    private async Task HandleSubscribeAsync(string socketId, SubscriptionRequest request)
    {
        if (request.ProductIds == null || !request.ProductIds.Any())
        {
            await SendErrorAsync(socketId, "No product IDs provided");
            return;
        }

        if (request.Channels == null || !request.Channels.Any())
        {
            await SendErrorAsync(socketId, "No channels provided");
            return;
        }

        // Get the subscription set for this socket
        if (!_subscriptions.TryGetValue(socketId, out var subscriptions))
        {
            subscriptions = new HashSet<string>();
            _subscriptions[socketId] = subscriptions;
        }

        // Process each channel and product combination
        foreach (var channel in request.Channels)
        {
            var channelName = channel;
            if (channel is JsonElement jsonElement)
            {
                // Handle both string and object channel formats
                if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    channelName = jsonElement.GetString() ?? "unknown";
                }
                else if (jsonElement.ValueKind == JsonValueKind.Object &&
                         jsonElement.TryGetProperty("name", out var nameElement))
                {
                    channelName = nameElement.GetString() ?? "unknown";
                }
                else
                {
                    await SendErrorAsync(socketId, "Invalid channel format");
                    continue;
                }
            }

            // Add subscriptions
            foreach (var productId in request.ProductIds)
            {
                string subscriptionKey = $"{channelName}:{productId}";
                subscriptions.Add(subscriptionKey);

                // Initialize last price for ticker channel
                if (channelName == "ticker" || channelName == "level2")
                {
                    try
                    {
                        // Create a scope to resolve the price service
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var priceService = scope.ServiceProvider.GetRequiredService<IPriceService>();
                            var price = await priceService.GetCurrentPriceAsync(productId);
                            _lastPrices[productId] = price;

                            // Send an initial update
                            await SendTickerUpdateAsync(socketId, productId, price);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error getting initial price for {ProductId}", productId);
                    }
                }
            }
        }

        // Send confirmation
        await SendMessageAsync(socketId, new
        {
            type = "subscriptions",
            channels = request.Channels
                .Select(c => c is JsonElement element && element.ValueKind == JsonValueKind.Object
                    ? JsonSerializer.Deserialize<object>(element.GetRawText())
                    : c)
                .ToList()
        });
    }

    /// <summary>
    /// Handles an unsubscribe request
    /// </summary>
    private async Task HandleUnsubscribeAsync(string socketId, SubscriptionRequest request)
    {
        if (!_subscriptions.TryGetValue(socketId, out var subscriptions))
        {
            return;
        }

        if (request.Channels == null || !request.Channels.Any())
        {
            // If no channels specified, unsubscribe from all
            subscriptions.Clear();
        }
        else
        {
            // Process each channel and product combination
            foreach (var channel in request.Channels)
            {
                var channelName = channel;
                if (channel is JsonElement jsonElement)
                {
                    // Handle both string and object channel formats
                    if (jsonElement.ValueKind == JsonValueKind.String)
                    {
                        channelName = jsonElement.GetString() ?? "unknown";
                    }
                    else if (jsonElement.ValueKind == JsonValueKind.Object &&
                             jsonElement.TryGetProperty("name", out var nameElement))
                    {
                        channelName = nameElement.GetString() ?? "unknown";
                    }
                    else
                    {
                        continue;
                    }
                }

                // Remove subscriptions
                if (request.ProductIds == null || !request.ProductIds.Any())
                {
                    // Remove all subscriptions for this channel
                    var keysToRemove = subscriptions
                        .Where(s => s.StartsWith($"{channelName}:"))
                        .ToList();

                    foreach (var key in keysToRemove)
                    {
                        subscriptions.Remove(key);
                    }
                }
                else
                {
                    // Remove specific product subscriptions
                    foreach (var productId in request.ProductIds)
                    {
                        string subscriptionKey = $"{channelName}:{productId}";
                        subscriptions.Remove(subscriptionKey);
                    }
                }
            }
        }

        // Send confirmation
        await SendMessageAsync(socketId, new
        {
            type = "subscriptions",
            channels = GetChannelSubscriptions(socketId)
        });
    }

    /// <summary>
    /// Get the current channel subscriptions for a socket
    /// </summary>
    private List<object> GetChannelSubscriptions(string socketId)
    {
        if (!_subscriptions.TryGetValue(socketId, out var subscriptions))
        {
            return new List<object>();
        }

        var channelGroups = subscriptions
            .Select(s => s.Split(':'))
            .Where(parts => parts.Length == 2)
            .GroupBy(parts => parts[0])
            .Select(g => new
            {
                name = g.Key,
                product_ids = g.Select(parts => parts[1]).ToList()
            })
            .ToList<object>();

        return channelGroups;
    }

    /// <summary>
    /// Monitors for price updates and broadcasts to subscribed clients
    /// </summary>
    private async Task MonitorPriceUpdatesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Build a unique list of all product IDs that any socket is subscribed to
                var productIds = new HashSet<string>();

                foreach (var kvp in _subscriptions)
                {
                    foreach (var subscription in kvp.Value)
                    {
                        var parts = subscription.Split(':');
                        if (parts.Length == 2 && (parts[0] == "ticker" || parts[0] == "level2"))
                        {
                            productIds.Add(parts[1]);
                        }
                    }
                }

                // Check for price updates for each product
                foreach (var productId in productIds)
                {
                    if (_lastPrices.TryGetValue(productId, out var lastPrice))
                    {
                        try
                        {
                            // Create a scope to resolve the price service
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var priceService = scope.ServiceProvider.GetRequiredService<IPriceService>();
                                var currentPrice = await priceService.GetCurrentPriceAsync(productId);

                                // Only send updates if the price has changed
                                if (currentPrice != lastPrice)
                                {
                                    _lastPrices[productId] = currentPrice;

                                    // Broadcast to all subscribed clients
                                    await BroadcastTickerUpdateAsync(productId, currentPrice);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error getting current price for {ProductId}", productId);
                        }
                    }
                }

                // Check every second
                await Task.Delay(1000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in price update monitoring");

                // Keep the background task running
                await Task.Delay(5000, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Broadcasts a ticker update to all subscribed clients
    /// </summary>
    private async Task BroadcastTickerUpdateAsync(string productId, decimal price)
    {
        foreach (var kvp in _subscriptions)
        {
            var socketId = kvp.Key;
            var subscriptions = kvp.Value;

            // Check if this socket is subscribed to ticker updates for this product
            if (subscriptions.Contains($"ticker:{productId}") || subscriptions.Contains($"level2:{productId}"))
            {
                await SendTickerUpdateAsync(socketId, productId, price);
            }
        }
    }

    /// <summary>
    /// Sends a ticker update to a specific client
    /// </summary>
    private async Task SendTickerUpdateAsync(string socketId, string productId, decimal price)
    {
        var tickerData = new
        {
            type = "ticker",
            product_id = productId,
            price = price.ToString(),
            time = DateTime.UtcNow.ToString("O")
        };

        await SendMessageAsync(socketId, tickerData);

        // If level2 is subscribed, also send an order book update
        if (_subscriptions.TryGetValue(socketId, out var subscriptions) && subscriptions.Contains($"level2:{productId}"))
        {
            // Generate a mock order book around the current price
            var bidPrice = price * 0.999m; // 0.1% below current price
            var askPrice = price * 1.001m; // 0.1% above current price

            // Create some random order book entries
            var random = new Random();
            var bids = new List<object>();
            var asks = new List<object>();

            // Generate 10 bid levels
            for (int i = 0; i < 10; i++)
            {
                var levelPrice = bidPrice * (1 - 0.0002m * i);
                var size = Math.Round(0.1m + (decimal)random.NextDouble() * 2, 6);
                bids.Add(new[] { levelPrice.ToString("F2"), size.ToString("F6") });
            }

            // Generate 10 ask levels
            for (int i = 0; i < 10; i++)
            {
                var levelPrice = askPrice * (1 + 0.0002m * i);
                var size = Math.Round(0.1m + (decimal)random.NextDouble() * 2, 6);
                asks.Add(new[] { levelPrice.ToString("F2"), size.ToString("F6") });
            }

            var orderBookData = new
            {
                type = "l2update",
                product_id = productId,
                changes = new
                {
                    bids,
                    asks
                },
                time = DateTime.UtcNow.ToString("O")
            };

            await SendMessageAsync(socketId, orderBookData);
        }
    }

    /// <summary>
    /// Sends heartbeats to all connected clients
    /// </summary>
    private void SendHeartbeats(object? state)
    {
        foreach (var socketId in _sockets.Keys)
        {
            _ = SendMessageAsync(socketId, new
            {
                type = "heartbeat",
                time = DateTime.UtcNow.ToString("O")
            });
        }
    }

    /// <summary>
    /// Sends an error message to a specific client
    /// </summary>
    private async Task SendErrorAsync(string socketId, string errorMessage)
    {
        await SendMessageAsync(socketId, new
        {
            type = "error",
            message = errorMessage
        });
    }

    /// <summary>
    /// Sends a message to a specific client
    /// </summary>
    private async Task SendMessageAsync(string socketId, object message)
    {
        if (!_sockets.TryGetValue(socketId, out var socket))
        {
            return;
        }

        try
        {
            var messageJson = JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);

            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(
                    new ArraySegment<byte>(messageBytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            else
            {
                // Socket is closed or in an invalid state
                RemoveSocket(socketId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WebSocket message to {SocketId}", socketId);
            RemoveSocket(socketId);
        }
    }

    /// <summary>
    /// Shuts down the WebSocket manager
    /// </summary>
    public void Shutdown()
    {
        _cancellationTokenSource.Cancel();
        _heartbeatTimer.Dispose();

        // Close all sockets
        foreach (var kvp in _sockets)
        {
            try
            {
                // Send a close message
                kvp.Value.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Server shutting down",
                    CancellationToken.None).Wait(1000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing WebSocket {SocketId}", kvp.Key);
            }
        }

        _sockets.Clear();
        _subscriptions.Clear();
        _lastPrices.Clear();
    }
}

public class SubscriptionRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("product_ids")]
    public List<string>? ProductIds { get; set; }

    [JsonPropertyName("channels")]
    public List<object>? Channels { get; set; }
}