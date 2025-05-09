# Coinbase Sandbox API

A local development sandbox for testing crypto trading bots against the Coinbase Advanced Trade API. This sandbox uses a hybrid approach:

- **Real Market Data**: Passes through product and pricing data from the real Coinbase API
- **Mock Trading**: Simulates order execution, wallet operations, and account balances locally

This gives you the best of both worlds - real market data with simulated trading.

## Features

- **Drop-in Replacement**: Just change the base URL in your client - no code changes needed
- **Uses Your Real API Keys**: The sandbox accepts and forwards your API keys for accessing real market data
- **Live Market Data**: Gets real-time product listings, prices, and market data from the actual Coinbase API
- **Sandbox Trading**: Simulates order execution and tracks wallet balances locally without risking real funds
- **Domain-Driven Design**: Implemented using DDD and hexagonal architecture for clean separation of concerns

## Getting Started

### Prerequisites

- .NET 9 SDK
- Your Coinbase Advanced Trade API bot client
- Valid Coinbase API key and secret (for accessing real market data)

### Installation

1. Clone this repository:
   ```
   git clone https://github.com/milesangelo/coinbase-sandbox.git
   cd coinbase-sandbox
   ```

2. Build and run the project:
   ```
   dotnet build
   dotnet run
   ```

3. The API will be available at:
   - HTTP: http://localhost:5226
   - HTTPS: https://localhost:7194

4. Browse to the Swagger documentation at:
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

## API Endpoints

The sandbox implements the same endpoints as the Coinbase Advanced Trade API:

### Passthrough Endpoints (Real Market Data)

- `GET /api/v3/brokerage/products` - Get all available products
- `GET /api/v3/brokerage/products/{product_id}` - Get a specific product
- `GET /api/v3/brokerage/products/{product_id}/candles` - Get price history
- `GET /api/v3/brokerage/products/{product_id}/ticker` - Get recent trades
- `GET /api/v3/brokerage/products/{product_id}/book` - Get order book

### Simulated Endpoints (Mock Trading)

- `POST /api/v3/brokerage/orders` - Place an order (simulated)
- `GET /api/v3/brokerage/orders/historical/{order_id}` - Get a specific order
- `GET /api/v3/brokerage/orders/historical` - Get all orders
- `POST /api/v3/brokerage/orders/batch_cancel` - Cancel orders
- `GET /api/v3/brokerage/accounts` - Get all accounts (simulated balances)

### Sandbox-Specific Endpoints

- `POST /api/v3/brokerage/sandbox/prices/{productId}` - Set a mock price for a product