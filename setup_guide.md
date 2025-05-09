# Setting Up Your Bot Client with the Coinbase Sandbox

This guide explains how to configure your trading bot to use the Coinbase Sandbox API, which gives you real market data but simulated trading.

## Client Configuration

Since the Coinbase Sandbox API uses the exact same routes and response formats as the real Coinbase API, the only change needed is to update the base URL while keeping your real API credentials.

### Example Configuration Change

```csharp
// config.json or appsettings.json

// Production config
{
  "CoinbaseApi": {
    "BaseUrl": "https://api.coinbase.com",
    "ApiKey": "your-real-api-key",
    "ApiSecret": "your-real-api-secret"
  }
}

// Sandbox config - just change the BaseUrl!
{
  "CoinbaseApi": {
    "BaseUrl": "http://localhost:5226",
    "ApiKey": "your-real-api-key",  // Keep your real API key
    "ApiSecret": "your-real-api-secret"  // Keep your real API secret
  }
}
```

### Switching Between Environments

You can use environment variables or configuration switches to easily toggle between sandbox and production:

```csharp
// Example in your bot client code
var baseUrl = Environment.GetEnvironmentVariable("USE_SANDBOX") == "true" 
    ? "http://localhost:5226" 
    : "https://api.coinbase.com";

// Keep using your real API key and secret
var client = new CoinbaseClient(baseUrl, realApiKey, realApiSecret);
```

### Example HttpClient Configuration

If your bot client uses HttpClient, it's as simple as changing the base address:

```csharp
// Production client
services.AddHttpClient<ICoinbaseClient, CoinbaseClient>(client => 
{
    client.BaseAddress = new Uri("https://api.coinbase.com");
    // Authentication headers are set on each request
});

// Sandbox client
services.AddHttpClient<ICoinbaseClient, CoinbaseClient>(client => 
{
    client.BaseAddress = new Uri("http://localhost:5226");
    // Authentication headers remain the same
});
```

## What You Get From This Hybrid Approach

### Real Market Data

- Your bot receives actual market prices, order books, and candles from the real Coinbase API
- Your strategy runs with the same market data it would in production
- This lets you see how your strategy would perform in real market conditions

### Simulated Trading

- Orders are simulated locally without touching your real funds
- Trade execution is simulated based on real-time market prices
- Account balances and order history are tracked locally

## Testing Specific Scenarios

The sandbox offers a special endpoint for testing specific price scenarios:

```csharp
// Example: Test a specific price point for BTC-USD
await httpClient.PostAsync(
    "http://localhost:5226/api/v3/brokerage/sandbox/prices/BTC-USD", 
    new StringContent("80000", Encoding.UTF8, "application/json"));
```

This allows you to override the real market price temporarily to test specific trading conditions.

## Example Strategy Test

Here's how you might test a strategy that buys when price drops below a moving average and sells when it rises above:

```csharp
[Test]
public async Task MovingAverageStrategy_Test()
{
    // Start with normal market data from real API
    // This will automatically use your API key/secret to get real data
    
    // Let the strategy run for a while with real market data
    await _tradingBot.ExecuteStrategyAsync();
    
    // Check if any orders were placed
    var orders = await _client.GetOrdersAsync();
    
    // Now simulate a specific price scenario
    await _client.SetMockPriceAsync("BTC-USD", 69000); // Below moving average
    
    // Run strategy again
    await _tradingBot.ExecuteStrategyAsync();
    
    // Verify a buy order was placed at this lower price
    var updatedOrders = await _client.GetOrdersAsync();
    Assert.That(updatedOrders.Count > orders.Count, "Should have placed a new order");
    Assert.That(updatedOrders.Last().Side == "buy", "Should have placed a buy order");
    
    // Simulate price rising above moving average
    await _client.SetMockPriceAsync("BTC-USD", 79000);
    
    // Run strategy again
    await _tradingBot.ExecuteStrategyAsync();
    
    // Verify a sell order was placed
    updatedOrders = await _client.GetOrdersAsync();
    Assert.That(updatedOrders.Last().Side == "sell", "Should have placed a sell order");
}
```

## Conclusion

With the Coinbase Sandbox API's hybrid approach, you get the best of both worlds:
- Real market data for accurate strategy testing
- Safe simulated trading without risking real funds
- Ability to test specific price scenarios

When you're ready to deploy to production, just change the base URL back to the real Coinbase API.

