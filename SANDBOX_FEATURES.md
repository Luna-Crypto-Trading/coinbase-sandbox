# Coinbase Sandbox - Feature Documentation

## 🎯 Overview

The Coinbase Sandbox is a complete, safe trading environment that simulates the Coinbase Advanced Trade API without using real money or connecting to live markets. It provides full API compatibility with the real Coinbase API while maintaining isolated, controllable state for testing and development.

## 🚀 Core Features

### ✅ **Wallet Management**
- **Default Wallet**: Pre-configured with initial balances (USD, BTC, ETH, SOL)
- **Custom Wallet Creation**: Create wallets with custom asset balances
- **Wallet Reset**: Reset wallet balances to custom values
- **Real-time Balance Updates**: Automatic balance adjustments after trades
- **Multi-Asset Support**: Support for multiple cryptocurrencies and fiat

### ✅ **Order Execution**
- **Market Orders**: Instant execution at current market prices
- **Buy Orders**: Purchase crypto with fiat currency
- **Sell Orders**: Sell crypto for fiat currency
- **Order Tracking**: Complete order history and status tracking
- **Fee Calculation**: Realistic trading fees (0.6% default rate)
- **Event Publishing**: Order events for real-time notifications

### ✅ **Price Management**
- **Mock Pricing**: Configurable asset prices for testing
- **Price Simulation**: Dynamic price movements (trends, volatility)
- **Real-time Updates**: Live price updates during simulations
- **Fallback Pricing**: Default prices when external data unavailable
- **Multi-Asset Pricing**: Support for BTC, ETH, SOL, and more

### ✅ **API Compatibility**
- **Coinbase API Format**: Matches real Coinbase Advanced Trade API
- **Standard Endpoints**: `/api/v3/brokerage/*` endpoints
- **Request/Response Format**: Identical to production API
- **Error Handling**: Proper HTTP status codes and error messages
- **Authentication Headers**: Accepts (but doesn't require) auth headers

## 📚 API Endpoints

### **Wallet Endpoints**

#### Get All Wallets
```http
GET /api/wallets
```
**Response:**
```json
[
  {
    "id": "default",
    "name": "Default Wallet",
    "assets": [
      {
        "currencySymbol": "USD",
        "currencyName": "US Dollar",
        "balance": 25000.00
      },
      {
        "currencySymbol": "BTC",
        "currencyName": "Bitcoin",
        "balance": 0.1
      }
    ]
  }
]
```

#### Get Specific Wallet
```http
GET /api/wallets/{walletId}
```

#### Get Asset Balance
```http
GET /api/wallets/{walletId}/assets/{symbol}
```

#### Deposit Funds (Sandbox Only)
```http
POST /api/wallets/{walletId}/assets/{symbol}/deposit
Content-Type: application/json

100.00
```

#### Withdraw Funds (Sandbox Only)
```http
POST /api/wallets/{walletId}/assets/{symbol}/withdraw
Content-Type: application/json

50.00
```

### **Trading Endpoints**

#### Create Order
```http
POST /api/v3/brokerage/orders
Content-Type: application/json

{
  "client_order_id": "test-buy-001",
  "product_id": "BTC-USD",
  "side": "BUY",
  "order_configuration": {
    "market_market_ioc": {
      "quote_size": "1000.00"
    }
  }
}
```

**Response:**
```json
{
  "success": true,
  "order": {
    "order_id": "2a17f4a2-3ed2-4509-afee-d55a3747cc3f",
    "client_order_id": "test-buy-001",
    "product_id": "BTC-USD",
    "side": "BUY",
    "status": "filled",
    "created_time": "2025-05-26T04:53:25.7178590Z"
  }
}
```

#### Get Order Details
```http
GET /api/v3/brokerage/orders/historical/{orderId}
```

**Response:**
```json
{
  "order": {
    "order_id": "2a17f4a2-3ed2-4509-afee-d55a3747cc3f",
    "product_id": "BTC-USD",
    "side": "buy",
    "order_type": "market",
    "status": "filled",
    "created_time": "2025-05-26T04:53:25.7178590Z",
    "base_size": "0.02",
    "limit_price": null,
    "filled_size": "0.02",
    "filled_price": "75000.00",
    "fee": "9.0000000"
  }
}
```

#### Get Order History
```http
GET /api/v3/brokerage/orders/historical?limit=100
```

### **Market Data Endpoints**

#### Get Products
```http
GET /api/v3/brokerage/products
```

#### Get Product Details
```http
GET /api/v3/brokerage/products/{productId}
```

#### Get Best Bid/Ask
```http
GET /api/v3/brokerage/best_bid_ask?product_ids=BTC-USD,ETH-USD
```

#### Get Accounts
```http
GET /api/v3/brokerage/accounts
```

### **Sandbox Management Endpoints**

#### Reset Wallet Balances
```http
POST /api/v3/sandbox/wallets/{walletId}/reset
Content-Type: application/json

[
  {
    "symbol": "USD",
    "name": "US Dollar",
    "balance": 25000.00
  },
  {
    "symbol": "BTC",
    "name": "Bitcoin",
    "balance": 0.1
  }
]
```

#### Set Static Price
```http
POST /api/v3/sandbox/prices/{productId}
Content-Type: application/json

75000.00
```

#### Start Price Simulation
```http
POST /api/v3/sandbox/prices/{productId}/simulate
Content-Type: application/json

{
  "mode": "volatility",
  "startPrice": 75000.00,
  "volatilityPercent": 5.0,
  "durationSeconds": 3600,
  "repeat": true
}
```

#### Get Sandbox State
```http
GET /api/v3/sandbox/state
```

#### Reset Entire Sandbox
```http
POST /api/v3/sandbox/reset
```

#### Setup Test Scenarios
```http
POST /api/v3/sandbox/scenarios
Content-Type: application/json

{
  "name": "bull_run",
  "prices": {
    "BTC-USD": 85000.00,
    "ETH-USD": 5000.00
  },
  "wallets": [
    {
      "id": "default",
      "assets": [
        {
          "symbol": "USD",
          "balance": 50000.00
        }
      ]
    }
  ]
}
```

## 🔧 Order Types Supported

### Market Orders
- **Market IOC (Immediate or Cancel)**: Execute immediately at current market price
- **Quote Size**: Specify amount in quote currency (e.g., USD)
- **Base Size**: Specify amount in base currency (e.g., BTC)

### Order Configuration Examples

#### Buy $1000 worth of BTC
```json
{
  "order_configuration": {
    "market_market_ioc": {
      "quote_size": "1000.00"
    }
  }
}
```

#### Sell 0.5 BTC
```json
{
  "order_configuration": {
    "market_market_ioc": {
      "base_size": "0.5"
    }
  }
}
```

## 💰 Fee Structure

- **Trading Fee**: 0.6% of trade value
- **Fee Calculation**: Applied to quote currency amount
- **Fee Deduction**: 
  - **Buy Orders**: Fee added to total cost
  - **Sell Orders**: Fee deducted from proceeds

### Example Fee Calculation
```
Buy Order: $1000 worth of BTC
- BTC Amount: 0.02 BTC (at $75,000/BTC = $1,500)
- Trading Fee: $1,500 × 0.6% = $9.00
- Total Cost: $1,500 + $9.00 = $1,509.00
```

## 🎮 Price Simulation Modes

### Static Mode
Set a fixed price for testing:
```json
{
  "mode": "static",
  "startPrice": 75000.00
}
```

### Trend Mode
Simulate price movement from start to end price:
```json
{
  "mode": "trend",
  "startPrice": 70000.00,
  "endPrice": 80000.00,
  "durationSeconds": 3600,
  "repeat": true
}
```

### Volatility Mode
Simulate random price fluctuations:
```json
{
  "mode": "volatility",
  "startPrice": 75000.00,
  "volatilityPercent": 10.0,
  "durationSeconds": 1800,
  "repeat": false
}
```

## 🧪 Test Scenarios

### Pre-built Scenarios
- **Bull Run**: Rising prices, optimistic market
- **Bear Market**: Falling prices, pessimistic market  
- **High Volatility**: Rapid price swings

### Custom Scenarios
Create your own test scenarios with:
- Custom asset prices
- Custom wallet balances
- Specific market conditions

## 🔄 Event System

### Order Events
- **OrderFilledEvent**: Published when orders execute
- **Real-time Notifications**: Events published immediately
- **Event IDs**: Unique identifiers for tracking

### Event Example
```
info: CoinbaseSandbox.Infrastructure.Services.InMemoryEventPublisher[0]
      Publishing event OrderFilledEvent with ID d04bd92a-20ef-4e16-9331-07ea2dfbf81e
```

## 🛡️ Safety Features

### Isolated Environment
- **No Real Money**: All transactions are simulated
- **No External Connections**: Operates independently
- **Resettable State**: Easy to reset to initial conditions

### Error Handling
- **Insufficient Funds**: Proper validation and error messages
- **Invalid Orders**: Comprehensive input validation
- **Network Errors**: Graceful handling of external API failures

## 🚦 Getting Started

### 1. Start the Application
```bash
dotnet run --project src/CoinbaseSandbox.Api --urls "http://localhost:5000"
```

### 2. Check Initial Wallet State
```bash
curl http://localhost:5000/api/wallets
```

### 3. Place Your First Order
```bash
curl -X POST http://localhost:5000/api/v3/brokerage/orders \
  -H "Content-Type: application/json" \
  -d '{
    "client_order_id": "my-first-order",
    "product_id": "BTC-USD", 
    "side": "BUY",
    "order_configuration": {
      "market_market_ioc": {
        "quote_size": "100.00"
      }
    }
  }'
```

### 4. Verify Balance Changes
```bash
curl http://localhost:5000/api/wallets
```

## 📊 Supported Assets

### Cryptocurrencies
- **BTC** (Bitcoin) - $75,000 default price
- **ETH** (Ethereum) - $4,500 default price  
- **SOL** (Solana) - $195 default price

### Fiat Currencies
- **USD** (US Dollar) - Base currency

### Trading Pairs
- **BTC-USD**: Bitcoin to US Dollar
- **ETH-USD**: Ethereum to US Dollar
- **SOL-USD**: Solana to US Dollar

## 🔍 Monitoring & Debugging

### Application Logs
- **Order Execution**: Detailed order processing logs
- **Price Updates**: Price change notifications
- **Error Tracking**: Comprehensive error logging
- **Event Publishing**: Real-time event notifications

### Health Checks
- **API Availability**: All endpoints respond correctly
- **Wallet State**: Balances update properly
- **Order Processing**: Orders execute successfully
- **Price Simulation**: Price updates work correctly

## 🎯 Use Cases

### Development Testing
- **API Integration**: Test your application against Coinbase API
- **Order Logic**: Validate trading algorithms
- **Error Handling**: Test edge cases and error conditions
- **Performance**: Load test your trading systems

### Education & Training
- **Learn Trading**: Practice trading without risk
- **API Familiarization**: Learn Coinbase API structure
- **Market Simulation**: Understand market dynamics
- **Strategy Testing**: Test trading strategies safely

### Quality Assurance
- **Automated Testing**: Integrate with test suites
- **Regression Testing**: Ensure features work consistently
- **Integration Testing**: Test full trading workflows
- **Scenario Testing**: Test specific market conditions

## 🔧 Configuration

### Default Settings
- **Initial USD Balance**: $10,000
- **Initial BTC Balance**: 0.5 BTC
- **Initial ETH Balance**: 5.0 ETH
- **Initial SOL Balance**: 20.0 SOL
- **Trading Fee Rate**: 0.6%

### Customizable Settings
- **Asset Prices**: Set any price for any asset
- **Wallet Balances**: Configure any starting balance
- **Fee Rates**: Adjust trading fees (via code)
- **Simulation Parameters**: Control price movements

## 📈 Future Enhancements

### Planned Features
- **Limit Orders**: Support for limit order types
- **Stop Orders**: Stop-loss and take-profit orders
- **Advanced Order Types**: More sophisticated order configurations
- **Portfolio Analytics**: Trading performance metrics
- **WebSocket Support**: Real-time price feeds
- **Historical Data**: Candlestick and trade history

### Integration Possibilities
- **Database Persistence**: Save state between sessions
- **External Price Feeds**: Real market data integration
- **User Management**: Multi-user support
- **API Rate Limiting**: Production-like constraints
- **Advanced Scenarios**: Complex market simulations

---

## 📞 Support

For questions, issues, or feature requests, please refer to the project documentation or create an issue in the repository.

**Happy Trading! 🚀** 