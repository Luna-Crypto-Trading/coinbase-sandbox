
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class WebSocketIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{

    [Fact]
    public async Task WebSocket_SubscribeToTicker_ReceivesTickerUpdates()
    {
        // Arrange
        var client = factory.CreateDefaultClient();
        var productId = "BTC-USD";
        var currentPrice = 50000m;

        // Set a static price through the API
        var priceResponse = await client.PostAsync(
            $"/api/v3/brokerage/sandbox/prices/{productId}",
            new StringContent(JsonSerializer.Serialize(currentPrice), Encoding.UTF8, "application/json"));

        priceResponse.EnsureSuccessStatusCode();

        // Create a WebSocket client
        var webSocketClient = factory.Server.CreateWebSocketClient();
        webSocketClient.ConfigureRequest = request =>
        {
            // No additional configuration needed
        };

        using var ws = await webSocketClient.ConnectAsync(
            new Uri($"{factory.Server.BaseAddress}ws".Replace("http", "ws")),
            CancellationToken.None);

        // Act - Send a subscription message
        var subscriptionMessage = new
        {
            type = "subscribe",
            product_ids = new[] { productId },
            channels = new[] { "ticker" }
        };

        await SendWebSocketMessageAsync(ws, subscriptionMessage);

        // Wait for multiple messages
        var messages = await ReceiveMultipleWebSocketMessagesAsync(ws, minimumCount: 2, timeout: TimeSpan.FromSeconds(10));

        // Assert
        Assert.NotEmpty(messages);

        // First message should be subscription confirmation
        var firstMessage = JsonSerializer.Deserialize<JsonElement>(messages[0]);
        Assert.Equal("subscriptions", firstMessage.GetProperty("type").GetString());

        // At least one message should be a ticker update
        var tickerMessage = messages.Skip(1)
            .Select(m => JsonSerializer.Deserialize<JsonElement>(m))
            .FirstOrDefault(m => m.GetProperty("type").GetString() == "ticker");

        Assert.NotEqual(default, tickerMessage);
        Assert.Equal(productId, tickerMessage.GetProperty("product_id").GetString());
        Assert.NotNull(tickerMessage.GetProperty("price").GetString());

        // Clean up - Unsubscribe
        var unsubscribeMessage = new
        {
            type = "unsubscribe",
            product_ids = new[] { productId },
            channels = new[] { "ticker" }
        };

        await SendWebSocketMessageAsync(ws, unsubscribeMessage);

        // Close the WebSocket
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", CancellationToken.None);
    }

    [Fact]
    public async Task WebSocket_SubscribeUnsubscribe_UpdatesSubscriptions()
    {
        // Arrange
        var client = factory.CreateDefaultClient();
        var productId = "ETH-USD";

        // Create a WebSocket client
        var webSocketClient = factory.Server.CreateWebSocketClient();
        using var ws = await webSocketClient.ConnectAsync(
            new Uri($"{factory.Server.BaseAddress}ws".Replace("http", "ws")),
            CancellationToken.None);

        // Act - Subscribe
        var subscribeMessage = new
        {
            type = "subscribe",
            product_ids = new[] { productId },
            channels = new[] { "ticker" }
        };

        await SendWebSocketMessageAsync(ws, subscribeMessage);

        // Verify subscription confirmation
        var subscribeResponse = await ReceiveWebSocketMessageAsync(ws, TimeSpan.FromSeconds(5));
        var subscribeJson = JsonSerializer.Deserialize<JsonElement>(subscribeResponse);

        Assert.Equal("subscriptions", subscribeJson.GetProperty("type").GetString());

        // Get at least one ticker message
        var tickerMessage = await WaitForMessageOfTypeAsync(ws, "ticker", TimeSpan.FromSeconds(5));
        Assert.NotNull(tickerMessage);

        // Act - Unsubscribe
        var unsubscribeMessage = new
        {
            type = "unsubscribe",
            product_ids = new[] { productId },
            channels = new[] { "ticker" }
        };

        await SendWebSocketMessageAsync(ws, unsubscribeMessage);

        // Verify unsubscription confirmation
        var unsubscribeResponse = await ReceiveWebSocketMessageAsync(ws, TimeSpan.FromSeconds(5));
        var unsubscribeJson = JsonSerializer.Deserialize<JsonElement>(unsubscribeResponse);

        Assert.Equal("subscriptions", unsubscribeJson.GetProperty("type").GetString());

        // Channels should be empty or not contain our subscription
        var channels = unsubscribeJson.GetProperty("channels").EnumerateArray();
        bool hasSubscription = false;

        foreach (var channel in channels)
        {
            if (channel.TryGetProperty("name", out var nameElement) &&
                nameElement.GetString() == "ticker" &&
                channel.TryGetProperty("product_ids", out var productIdsElement))
            {
                foreach (var id in productIdsElement.EnumerateArray())
                {
                    if (id.GetString() == productId)
                    {
                        hasSubscription = true;
                        break;
                    }
                }
            }
        }

        Assert.False(hasSubscription);

        // Clean up
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", CancellationToken.None);
    }

    [Fact]
    public async Task WebSocket_SetPrice_TriggersTickerUpdate()
    {
        // Arrange
        var client = factory.CreateDefaultClient();
        var productId = "BTC-USD";
        var startingPrice = 60000m;
        var newPrice = 65000m;

        // Set initial price
        var initialPriceResponse = await client.PostAsync(
            $"/api/v3/brokerage/sandbox/prices/{productId}",
            new StringContent(JsonSerializer.Serialize(startingPrice), Encoding.UTF8, "application/json"));

        initialPriceResponse.EnsureSuccessStatusCode();

        // Create a WebSocket client
        var webSocketClient = factory.Server.CreateWebSocketClient();
        using var ws = await webSocketClient.ConnectAsync(
            new Uri($"{factory.Server.BaseAddress}ws".Replace("http", "ws")),
            CancellationToken.None);

        // Subscribe to ticker updates
        var subscribeMessage = new
        {
            type = "subscribe",
            product_ids = new[] { productId },
            channels = new[] { "ticker" }
        };

        await SendWebSocketMessageAsync(ws, subscribeMessage);

        // Skip the subscription confirmation
        await ReceiveWebSocketMessageAsync(ws, TimeSpan.FromSeconds(5));

        // Get the initial ticker message
        var initialTickerMessage = await WaitForMessageOfTypeAsync(ws, "ticker", TimeSpan.FromSeconds(5));
        var initialTickerJson = JsonSerializer.Deserialize<JsonElement>(initialTickerMessage);
        var initialTickerPrice = decimal.Parse(initialTickerJson.GetProperty("price").GetString());

        // Act - Change the price
        var updatePriceResponse = await client.PostAsync(
            $"/api/v3/brokerage/sandbox/prices/{productId}",
            new StringContent(JsonSerializer.Serialize(newPrice), Encoding.UTF8, "application/json"));

        updatePriceResponse.EnsureSuccessStatusCode();

        // Wait for the new ticker message with updated price
        var updatedTickerMessage = await WaitForConditionAsync(ws, message =>
        {
            var json = JsonSerializer.Deserialize<JsonElement>(message);
            if (json.TryGetProperty("type", out var typeElement) &&
                typeElement.GetString() == "ticker" &&
                json.TryGetProperty("product_id", out var productElement) &&
                productElement.GetString() == productId &&
                json.TryGetProperty("price", out var priceElement))
            {
                var price = decimal.Parse(priceElement.GetString());
                return price == newPrice;
            }
            return false;
        }, TimeSpan.FromSeconds(10));

        // Assert
        Assert.NotNull(updatedTickerMessage);
        var updatedTickerJson = JsonSerializer.Deserialize<JsonElement>(updatedTickerMessage);
        var updatedTickerPrice = decimal.Parse(updatedTickerJson.GetProperty("price").GetString());

        Assert.Equal(newPrice, updatedTickerPrice);

        // Clean up
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", CancellationToken.None);
    }

    #region Helper Methods

    private static async Task SendWebSocketMessageAsync(WebSocket webSocket, object message)
    {
        var json = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(json);
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    private static async Task<string> ReceiveWebSocketMessageAsync(WebSocket webSocket, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var buffer = new byte[8192];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            throw new InvalidOperationException("WebSocket was closed");
        }

        return Encoding.UTF8.GetString(buffer, 0, result.Count);
    }

    private static async Task<List<string>> ReceiveMultipleWebSocketMessagesAsync(
        WebSocket webSocket,
        int minimumCount,
        TimeSpan timeout)
    {
        var messages = new List<string>();
        var startTime = DateTime.UtcNow;

        while (messages.Count < minimumCount && DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var remainingTime = timeout - (DateTime.UtcNow - startTime);
                if (remainingTime <= TimeSpan.Zero)
                {
                    break;
                }

                var message = await ReceiveWebSocketMessageAsync(webSocket, remainingTime);
                messages.Add(message);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        return messages;
    }

    private static async Task<string> WaitForMessageOfTypeAsync(
        WebSocket webSocket,
        string messageType,
        TimeSpan timeout)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var remainingTime = timeout - (DateTime.UtcNow - startTime);
                if (remainingTime <= TimeSpan.Zero)
                {
                    break;
                }

                var message = await ReceiveWebSocketMessageAsync(webSocket, remainingTime);
                var json = JsonSerializer.Deserialize<JsonElement>(message);

                if (json.TryGetProperty("type", out var typeElement) &&
                    typeElement.GetString() == messageType)
                {
                    return message;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        return null;
    }

    private static async Task<string> WaitForConditionAsync(
        WebSocket webSocket,
        Func<string, bool> condition,
        TimeSpan timeout)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var remainingTime = timeout - (DateTime.UtcNow - startTime);
                if (remainingTime <= TimeSpan.Zero)
                {
                    break;
                }

                var message = await ReceiveWebSocketMessageAsync(webSocket, remainingTime);

                if (condition(message))
                {
                    return message;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        return null;
    }

    #endregion
}