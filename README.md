# Coinbase Sandbox API

A local development sandbox for testing crypto trading bots against the Coinbase Advanced Trade API. This sandbox uses a hybrid approach:

- **Real Market Data**: Passes through product and pricing data from the real Coinbase API
- **Mock Trading**: Simulates order execution, wallet operations, and account balances locally

This gives you the best of both worlds - real market data with simulated trading.

## Quick Start with Docker

```bash
# Pull and run the sandbox
docker run -p 5226:5226 ghcr.io/milesangelo/coinbase-sandbox:latest

# Point your bot to http://localhost:5226 instead of https://api.coinbase.com
```

## Docker Compose Example

```yaml
version: '3.8'

services:
  coinbase-sandbox:
    image: ghcr.io/milesangelo/coinbase-sandbox:latest
    container_name: coinbase-sandbox
    ports:
      - "5226:5226"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5226
    restart: unless-stopped
    
  # Example connecting with your trading bot
  trading-bot:
    image: your-trading-bot-image:latest
    environment:
      - COINBASE_API_URL=http://coinbase-sandbox:5226
      - COINBASE_API_KEY=${COINBASE_API_KEY}
      - COINBASE_API_SECRET=${COINBASE_API_SECRET}
    depends_on:
      - coinbase-sandbox
```

## 📖 Documentation

- **[Complete Feature Documentation](SANDBOX_FEATURES.md)** - Comprehensive guide to all sandbox features and API endpoints
- **[Setup Guide](setup_guide.md)** - Detailed setup and configuration instructions
- **[WebSocket Tester](websocker-tester.html)** - Interactive WebSocket testing tool

## Features

### 🔄 API Compatibility

- **Drop-in Replacement**: Just change the base URL in your client - no code changes needed
- **Identical API Endpoints**: Mimics the exact same routes and response formats as the real Coinbase Advanced Trade API
- **Authentication Support**: Uses your real API keys for market data access (but never touches your real funds)

### 📈 Market Data

- **Real-time Prices**: Get actual market prices from the Coinbase API
- **Product Listings**: Access the complete catalog of tradable assets
- **Candlestick Data**: Historical price data with the same format as production
- **Order Book Depth**: Real market order book data for testing your strategies

### 💰 Simulated Trading

- **Virtual Account Balances**: Start with configurable virtual balances
- **Order Execution**: Place market and limit orders that execute against real-time prices
- **Trade History**: Track all your simulated trades
- **Fee Calculation**: Realistic fee calculation for accurate P&L testing

### 📊 Analysis & Testing

- **Technical Indicators**: Calculate SMAs, EMAs, RSI, Bollinger Bands and more
- **Backtesting Engine**: Test trading strategies against historical data
- **Price Simulation**: Create custom price scenarios (trend, volatility, replay)
- **Scenario Testing**: Pre-built scenarios for bull markets, bear markets, and high volatility

### 🔌 WebSockets

- **Real-time Updates**: WebSocket connection mimicking Coinbase's service
- **Price Tickers**: Subscribe to real-time price updates
- **Order Book Updates**: Level 2 market data streaming
- **Interactive Tester**: Browser-based WebSocket testing tool

### 🔍 Debugging & Monitoring

- **Sandbox Dashboard**: Visualize current state and trading activity
- **State Management**: View and reset the sandbox state
- **Transaction Log**: Record of all simulated trading activity
- **Notification System**: Trade and price alert notifications

## Getting Started

### Prerequisites

- .NET 9 SDK (for building from source)
- Your Coinbase Advanced Trade API bot client
- Valid Coinbase API key and secret (for accessing real market data)

### Installation Options

#### Option 1: Docker (Recommended)

```bash
docker run -p 5226:5226 ghcr.io/milesangelo/coinbase-sandbox:latest
```

#### Option 2: Build from Source

1. Clone this repository:
   ```
   git clone https://github.com/milesangelo/coinbase-sandbox.git
   cd coinbase-sandbox
   ```

2. Build and run the project:
   ```
   dotnet build
   dotnet run --project src/CoinbaseSandbox.Api
   ```

3. The API will be available at:
   - HTTP: http://localhost:5226
   - HTTPS: https://localhost:7194

4. Access the Swagger documentation at:
   - HTTP: http://localhost:5226/swagger
   - HTTPS: https://localhost:7194/swagger

### Usage

1. **Configure your trading bot** to point to the sandbox API:
   - Instead of using `https://api.coinbase.com` as the base URL, use `http://localhost:5226`
   - Keep using your real Coinbase API key and secret - the sandbox will forward these for market data
   - No other changes are needed since the routes match exactly

2. **Get real market data**:
   - The sandbox passes through product listings, price data, candles, and order book data from the real Coinbase API
   - Your API key and secret are used to authenticate these passthrough requests
   - This gives you accurate, real-time market data for testing strategies

3. **Execute simulated trades**:
   - The sandbox intercepts order creation, cancellation, and account balance endpoints
   - It simulates trade execution locally using the current market price
   - Your real Coinbase account and funds are never touched

4. **Test with mock prices**:
   - For specific testing scenarios, use the special sandbox endpoint to override prices
   - `POST /api/v3/brokerage/sandbox/prices/{productId}` with a price value

5. **Try the WebSocket tester**:
   - Open http://localhost:5226/websocket-tester.html in your browser
   - Connect to the WebSocket server and subscribe to price updates
   - Visualize real-time price changes and order book updates

6. **Access the Dashboard**:
   - Navigate to http://localhost:5226/dashboard.html
   - View account balances, order history, and price charts
   - Monitor the state of your sandbox

## API Endpoints

The sandbox implements the same endpoints as the Coinbase Advanced Trade API plus additional sandbox-specific endpoints:

### Standard Coinbase API Endpoints (Passthrough)

- `GET /api/v3/brokerage/products` - Get all available products
- `GET /api/v3/brokerage/products/{product_id}` - Get a specific product
- `GET /api/v3/brokerage/products/{product_id}/candles` - Get price history
- `GET /api/v3/brokerage/products/{product_id}/ticker` - Get recent trades
- `GET /api/v3/brokerage/products/{product_id}/book` - Get order book

### Standard Coinbase API Endpoints (Simulated)

- `POST /api/v3/brokerage/orders` - Place an order (simulated)
- `GET /api/v3/brokerage/orders/historical/{order_id}` - Get a specific order
- `GET /api/v3/brokerage/orders/historical` - Get all orders
- `POST /api/v3/brokerage/orders/batch_cancel` - Cancel orders
- `GET /api/v3/brokerage/accounts` - Get all accounts (simulated balances)

### Sandbox-Specific Endpoints

#### Price Control

- `POST /api/v3/brokerage/sandbox/prices/{productId}` - Set a static price
- `POST /api/v3/sandbox/prices/{productId}/simulate` - Simulate price movements (trend/volatility)
- `DELETE /api/v3/sandbox/prices/{productId}/simulation` - Stop price simulation

#### Wallet Management

- `POST /api/v3/sandbox/wallets` - Create a custom wallet with specified balances
- `POST /api/v3/sandbox/wallets/{walletId}/reset` - Reset wallet balances

#### Scenario Testing

- `POST /api/v3/sandbox/scenarios` - Set up predefined test scenarios (bull/bear/volatile)

#### System State

- `POST /api/v3/sandbox/reset` - Reset the entire sandbox to initial state
- `GET /api/v3/sandbox/state` - Get the current state of the sandbox

#### Technical Analysis

- `GET /api/technical-analysis/{productId}/indicators` - Get all technical indicators
- `GET /api/technical-analysis/{productId}/sma` - Calculate Simple Moving Averages
- `GET /api/technical-analysis/{productId}/ema` - Calculate Exponential Moving Averages
- `GET /api/technical-analysis/{productId}/rsi` - Calculate Relative Strength Index
- `GET /api/technical-analysis/{productId}/bollinger-bands` - Calculate Bollinger Bands

#### Backtesting

- `POST /api/backtest/run` - Run a backtest with a specific strategy
- `GET /api/backtest/strategies` - Get available backtest strategies
- `GET /api/backtest/results` - Get backtest results
- `GET /api/backtest/results/{id}` - Get specific backtest result

#### Notifications

- `POST /api/notifications/subscribe/price-alerts` - Subscribe to price alerts
- `DELETE /api/notifications/unsubscribe/price-alerts/{productId}` - Unsubscribe from alerts

### WebSocket API

The sandbox implements a WebSocket server at `/ws` that mimics the Coinbase WebSocket API. It supports:

- Price ticker updates
- Level 2 order book updates
- Heartbeat messages

## How It Works

The sandbox works by:

1. **Passing Through Market Data**:
   - Forwards your authentication headers to the real Coinbase API for product and market data endpoints
   - Returns real-time market data to your trading bot

2. **Intercepting Trading Endpoints**:
   - Captures order creation/cancellation requests
   - Gets the current market price from the real API
   - Simulates order execution and balance updates locally
   - Returns responses that match the format of the real API

3. **Maintaining Local State**:
   - Tracks your simulated account balances and order history
   - Allows you to view your orders and account state just like the real API

4. **Price Simulation**:
   - Can run static prices, trends, or simulate volatility
   - Broadcasts price updates via WebSockets to connected clients

## Testing Specific Scenarios

The sandbox offers endpoints for testing specific price scenarios:

```csharp
// Example: Test a specific price point for BTC-USD
await httpClient.PostAsync(
    "http://localhost:5226/api/v3/brokerage/sandbox/prices/BTC-USD", 
    new StringContent("80000", Encoding.UTF8, "application/json"));

// Example: Simulate a bull market trend
await httpClient.PostAsync(
    "http://localhost:5226/api/v3/sandbox/prices/BTC-USD/simulate", 
    new StringContent(JsonSerializer.Serialize(new {
        mode = "trend",
        startPrice = 70000,
        endPrice = 80000,
        durationSeconds = 3600,
        repeat = true
    }), Encoding.UTF8, "application/json"));
```

## Architecture

The project is built using Clean Architecture principles with Domain-Driven Design:

- **Domain Layer**: Core business logic and entities
- **Application Layer**: Use cases and application services
- **Infrastructure Layer**: External services and data persistence
- **API Layer**: HTTP endpoints and controllers

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- This project is not affiliated with or endorsed by Coinbase
- Use at your own risk for testing purposes only
