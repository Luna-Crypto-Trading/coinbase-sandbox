# Product Requirements Document: Coinbase Sandbox Management UI

## 🎯 Executive Summary

### **Product Vision**
Create a comprehensive web-based management interface for the Coinbase Sandbox API that enables developers, traders, and QA teams to easily test, configure, and monitor their trading applications in a safe, simulated environment.

### **Core Value Proposition**
- **Zero-Code Testing**: Test Coinbase API integration without writing test scripts
- **Visual Scenario Management**: Easily create and run market scenarios
- **Real-time Monitoring**: Live view of WebSocket data, trades, and balances
- **API Key Validation**: Verify Coinbase credentials work correctly
- **Complete Sandbox Control**: Full management of prices, wallets, and market conditions

---

## 📊 Market Context & User Needs

### **Target Users**

#### **Primary Users**
1. **Trading Bot Developers** - Need to test algorithms against various market conditions
2. **QA Engineers** - Require comprehensive API testing capabilities  
3. **Financial Application Developers** - Building apps that integrate with Coinbase
4. **Crypto Trading Educators** - Teaching trading concepts safely

#### **Secondary Users**
1. **DevOps Teams** - Monitoring sandbox health and performance
2. **Product Managers** - Demonstrating trading features to stakeholders
3. **Support Teams** - Troubleshooting integration issues

### **Current Pain Points**
- **Command Line Complexity**: Current testing requires curl commands and technical knowledge
- **No Visual Feedback**: Difficult to see real-time impact of changes
- **Scattered Testing**: No centralized place to test all API endpoints
- **Manual Scenario Setup**: Time-consuming to recreate market conditions
- **WebSocket Debugging**: Hard to visualize real-time data streams

---

## 🚀 Product Requirements

## **Feature 1: Sandbox Dashboard & Overview**

### **F1.1 Real-time Sandbox State**
**Priority: HIGH**

**Requirements:**
- Display current wallet balances for all assets
- Show active price simulations and their status
- Display recent order history (last 50 orders)
- Show current mock prices vs real market prices
- Real-time updates without page refresh

**Acceptance Criteria:**
- ✅ Dashboard loads in under 2 seconds
- ✅ All data updates in real-time via WebSocket
- ✅ Shows clear distinction between mock and real prices
- ✅ Wallet balances update immediately after trades

### **F1.2 System Health Monitoring**
**Priority: MEDIUM**

**Requirements:**
- API endpoint health status indicators
- WebSocket connection status
- Recent API call logs and response times
- Error rate and success metrics

---

## **Feature 2: Market Scenario Management**

### **F2.1 Scenario Builder Interface**
**Priority: HIGH**

**Requirements:**
- **Pre-built Scenarios**: Bull Run, Bear Market, High Volatility, Flash Crash
- **Custom Scenario Creator**: 
  - Set custom prices for multiple assets
  - Configure wallet balances
  - Set market conditions (trend, volatility)
- **Scenario Templates**: Save and reuse custom scenarios
- **One-Click Deployment**: Apply scenarios instantly

**User Interface:**
```
┌─────────────────────────────────────────┐
│ 📊 Market Scenarios                     │
├─────────────────────────────────────────┤
│ Quick Scenarios:                        │
│ [🐂 Bull Run] [🐻 Bear Market]          │
│ [📈 High Vol] [⚡ Flash Crash]          │
│                                         │
│ Custom Scenario Builder:                │
│ Asset Prices:                           │
│ BTC-USD: [75000] [$] [Set Price]       │
│ ETH-USD: [4500]  [$] [Set Price]       │
│                                         │  
│ Wallet Balances:                        │
│ USD: [25000] [💰] [Update]              │
│ BTC: [0.5]   [₿]  [Update]              │
│                                         │
│ [💾 Save Scenario] [🚀 Deploy Now]      │
└─────────────────────────────────────────┘
```

### **F2.2 Price Simulation Controls**
**Priority: HIGH**

**Requirements:**
- **Static Price Setting**: Set fixed prices for testing
- **Trend Simulation**: Configure start/end prices and duration
- **Volatility Simulation**: Set base price and volatility percentage
- **Real-time Controls**: Start, stop, pause simulations
- **Visual Progress**: Show simulation progress and current values

**User Interface:**
```
┌─────────────────────────────────────────┐
│ 🎛️ Price Simulation: BTC-USD           │
├─────────────────────────────────────────┤
│ Current Price: $75,250.00               │
│ Real Price:    $75,180.00               │
│                                         │
│ Simulation Mode:                        │
│ ○ Static  ● Trend  ○ Volatility        │
│                                         │
│ Start Price:  [70000] [$]               │
│ End Price:    [80000] [$]               │
│ Duration:     [30] [minutes]            │
│ Repeat:       ☑️ Yes                    │
│                                         │
│ [▶️ Start] [⏸️ Pause] [⏹️ Stop]          │
│                                         │
│ Progress: ████████░░ 80% (4min left)    │
└─────────────────────────────────────────┘
```

---

## **Feature 3: API Endpoint Testing Suite**

### **F3.1 Interactive API Explorer**
**Priority: HIGH**

**Requirements:**
- **Endpoint Directory**: Organized list of all Advanced Trade API endpoints
- **Request Builder**: Visual form to build API requests
- **Response Viewer**: Formatted JSON responses with syntax highlighting
- **Request History**: Save and replay previous requests
- **Code Generation**: Generate curl, Python, JavaScript examples

**Organization Structure:**
```
📁 Products
  ├── GET /api/v3/brokerage/products
  ├── GET /api/v3/brokerage/products/{id}
  └── GET /api/v3/brokerage/best_bid_ask

📁 Orders  
  ├── POST /api/v3/brokerage/orders
  ├── GET /api/v3/brokerage/orders/historical
  └── DELETE /api/v3/brokerage/orders/{id}

📁 Accounts
  ├── GET /api/v3/brokerage/accounts
  └── GET /api/v3/brokerage/accounts/{id}
```

### **F3.2 Order Testing Interface**
**Priority: HIGH**

**Requirements:**
- **Visual Order Builder**: Form-based order creation
- **Order Types Support**: Market, Limit (when implemented)
- **Real-time Execution**: Show order processing in real-time
- **Order Book Display**: Visual representation of current orders
- **Trade History**: Chronological list of executed trades

**User Interface:**
```
┌─────────────────────────────────────────┐
│ 📋 Create Order                         │
├─────────────────────────────────────────┤
│ Product:    [BTC-USD ▼]                 │
│ Side:       ● Buy  ○ Sell               │
│ Order Type: ● Market  ○ Limit           │
│                                         │
│ Amount Type: ● Quote Size  ○ Base Size  │
│ Amount:     [1000] [USD]                │
│                                         │
│ Client Order ID: [my-test-order-001]    │
│                                         │
│ [🚀 Place Order]                        │
│                                         │
│ Estimated:                              │
│ • You'll receive: ~0.0133 BTC           │
│ • Trading fee: $6.00                    │
│ • Total cost: $1,006.00                 │
└─────────────────────────────────────────┘
```

---

## **Feature 4: WebSocket Data Visualization**

### **F4.1 Real-time Data Streams**
**Priority: HIGH**

**Requirements:**
- **Connection Manager**: Connect/disconnect to WebSocket feeds
- **Channel Subscriptions**: Subscribe to specific data channels
- **Live Data Display**: Real-time price tickers, order book updates
- **Message Inspector**: View raw WebSocket messages
- **Data Export**: Save WebSocket data for analysis

**Channel Support:**
- Price tickers for all products
- Level 2 order book data
- User order updates
- Heartbeat messages

### **F4.2 Interactive Charts**
**Priority: MEDIUM**

**Requirements:**
- **Real-time Price Charts**: Candlestick and line charts
- **Order Book Visualization**: Depth chart showing bid/ask levels
- **Trade Flow**: Visual representation of order executions
- **Multiple Timeframes**: 1m, 5m, 15m, 1h chart intervals

---

## **Feature 5: API Key Management & Testing**

### **F5.1 Credential Validation**
**Priority: HIGH**

**Requirements:**
- **API Key Input**: Secure form for entering Coinbase credentials
- **Connection Testing**: Verify keys work with real Coinbase API
- **Permission Checking**: Display what permissions the keys have
- **Rate Limit Monitoring**: Show current rate limit usage
- **Key Storage**: Temporary session storage (never persisted)

**User Interface:**
```
┌─────────────────────────────────────────┐
│ 🔐 API Key Testing                      │
├─────────────────────────────────────────┤
│ API Key:    [••••••••••••••••••••••••]  │
│ API Secret: [••••••••••••••••••••••••]  │
│ Passphrase: [••••••••••••••••••••••••]  │
│                                         │
│ [🧪 Test Connection]                    │
│                                         │
│ Status: ✅ Connected Successfully        │
│ Permissions: ✅ View ✅ Trade ❌ Transfer │
│ Rate Limit: 8/10 requests used         │
│                                         │
│ Last Test: 2 minutes ago                │
│ ⚠️  Keys stored in session only         │
└─────────────────────────────────────────┘
```

### **F5.2 Passthrough Testing**
**Priority: HIGH**

**Requirements:**
- **Real API Calls**: Test actual Coinbase API endpoints using user's keys
- **Response Comparison**: Compare real API vs sandbox responses
- **Market Data Verification**: Ensure passthrough data is accurate
- **Error Handling**: Test various error scenarios

---

## **Feature 6: Wallet & Portfolio Management**

### **F6.1 Wallet Configuration**
**Priority: HIGH**

**Requirements:**
- **Multi-Wallet Support**: Create and manage multiple test wallets
- **Balance Adjustment**: Add/remove funds from any asset
- **Wallet Templates**: Pre-configured wallet setups
- **Balance History**: Track balance changes over time
- **Reset Functionality**: One-click wallet reset to defaults

### **F6.2 Portfolio Analytics**
**Priority: MEDIUM**

**Requirements:**
- **P&L Tracking**: Show profit/loss from trades
- **Asset Allocation**: Pie chart of current holdings
- **Performance Metrics**: ROI, Sharpe ratio, win rate
- **Trade Analytics**: Best/worst trades, average hold time

---

## **Feature 7: Technical Analysis & Backtesting**

### **F7.1 Technical Indicators Dashboard**
**Priority: MEDIUM**

**Requirements:**
- **Indicator Calculator**: Input parameters for SMA, EMA, RSI, etc.
- **Visual Charts**: Plot indicators on price charts  
- **Signal Detection**: Highlight buy/sell signals
- **Custom Indicators**: Allow users to create custom calculations

### **F7.2 Strategy Backtesting Interface**
**Priority: MEDIUM**

**Requirements:**
- **Strategy Builder**: Visual interface for creating trading strategies
- **Backtest Runner**: Execute strategies against historical data
- **Results Analysis**: Performance metrics and charts
- **Strategy Comparison**: Compare multiple strategies side-by-side

---

## 🎨 Technical Architecture

### **Frontend Technology Stack**
- **Framework**: React 18 with TypeScript
- **Styling**: Tailwind CSS + Headless UI components
- **Charts**: TradingView Lightweight Charts or Chart.js
- **WebSocket**: Native WebSocket API with reconnection logic
- **State Management**: Zustand or React Query
- **Build Tool**: Vite for fast development

### **UI Component Structure**
```
src/
├── components/
│   ├── dashboard/
│   │   ├── SandboxOverview.tsx
│   │   ├── WalletBalances.tsx
│   │   └── RecentOrders.tsx
│   ├── scenarios/
│   │   ├── ScenarioBuilder.tsx
│   │   ├── PriceSimulator.tsx
│   │   └── QuickScenarios.tsx
│   ├── api-testing/
│   │   ├── EndpointExplorer.tsx
│   │   ├── OrderBuilder.tsx
│   │   └── ResponseViewer.tsx
│   ├── websocket/
│   │   ├── ConnectionManager.tsx
│   │   ├── DataStreams.tsx
│   │   └── MessageInspector.tsx
│   └── shared/
│       ├── Chart.tsx
│       ├── JsonViewer.tsx
│       └── StatusIndicator.tsx
├── hooks/
│   ├── useWebSocket.ts
│   ├── useSandboxApi.ts
│   └── useRealTimeData.ts
├── services/
│   ├── sandboxApi.ts
│   ├── coinbaseApi.ts
│   └── websocketService.ts
└── types/
    ├── sandbox.ts
    ├── coinbase.ts
    └── common.ts
```

### **API Integration**
- **Sandbox API**: Full integration with all existing controllers
- **Real-time Updates**: WebSocket connection for live data
- **Error Handling**: Comprehensive error boundaries and user feedback
- **Loading States**: Skeleton loaders and progress indicators

---

## 📱 User Experience Design

### **Navigation Structure**
```
🏠 Dashboard
├── 📊 Overview
├── 💰 Wallets  
└── 📈 Recent Activity

🎭 Scenarios
├── 🚀 Quick Deploy
├── 🛠️ Custom Builder
└── 💾 Saved Templates

🔧 API Testing
├── 📋 Endpoints
├── 🛒 Order Builder
└── 📜 Request History

📡 WebSocket
├── 🔌 Connections
├── 📊 Live Data
└── 🔍 Message Inspector

🔐 Settings
├── 🔑 API Keys
├── ⚙️ Configuration
└── 📊 Monitoring
```

### **Responsive Design**
- **Desktop First**: Optimized for development workflows
- **Tablet Support**: Key features accessible on tablets
- **Mobile Aware**: Basic monitoring capabilities on mobile

### **Dark/Light Theme**
- **Professional Dark Theme**: Default for developers
- **Light Theme Option**: For presentations and demos
- **System Preference**: Auto-detect user preference

---

## 🎯 Success Metrics

### **User Engagement**
- **Daily Active Users**: Target 50+ developers using the UI daily
- **Session Duration**: Average 15+ minutes per session
- **Feature Adoption**: 80% of users utilize at least 3 major features

### **Functionality Metrics**
- **API Test Success Rate**: 95%+ successful API calls
- **Scenario Deployment Time**: Under 30 seconds average
- **WebSocket Connection Stability**: 99%+ uptime

### **Developer Experience**
- **Time to First Trade**: Under 5 minutes for new users
- **Support Ticket Reduction**: 40% fewer API integration questions
- **User Satisfaction**: 4.5+ star rating from developer feedback

---

## 📅 Development Roadmap

### **Phase 1: Core Foundation (Weeks 1-3)**
- ✅ Project setup and basic routing
- ✅ Dashboard overview with real-time data
- ✅ Basic wallet management interface
- ✅ WebSocket connection and basic data display

### **Phase 2: Scenario Management (Weeks 4-6)**
- ✅ Scenario builder interface
- ✅ Price simulation controls
- ✅ Quick scenario deployment
- ✅ Custom scenario templates

### **Phase 3: API Testing Suite (Weeks 7-9)**
- ✅ Endpoint explorer and request builder
- ✅ Order testing interface
- ✅ Response viewer with syntax highlighting
- ✅ Request history and code generation

### **Phase 4: Advanced Features (Weeks 10-12)**
- ✅ API key testing and validation
- ✅ Advanced WebSocket features
- ✅ Technical analysis integration
- ✅ Portfolio analytics

### **Phase 5: Polish & Launch (Weeks 13-14)**
- ✅ Comprehensive testing and bug fixes
- ✅ Documentation and user guides
- ✅ Performance optimization
- ✅ Launch preparation

---

## 🔒 Security Considerations

### **API Key Handling**
- **Session Storage Only**: Never persist API keys to disk
- **Secure Transmission**: All API calls over HTTPS
- **Clear on Close**: Automatically clear keys when browser closes
- **Warning Messages**: Clear notifications about key security

### **Data Privacy**
- **No User Tracking**: No analytics or user behavior tracking
- **Local Processing**: All data processing happens client-side
- **Temporary Storage**: Clear all data on session end

---

## 🚀 Future Enhancements

### **Phase 2 Features**
- **Multi-User Support**: Team collaboration features
- **Advanced Analytics**: Detailed performance reporting
- **Strategy Marketplace**: Share and download trading strategies
- **Mobile App**: Native mobile app for monitoring

### **Integration Possibilities**
- **IDE Plugins**: VS Code extension for in-editor testing
- **CI/CD Integration**: Automated testing in deployment pipelines
- **Webhook Support**: External notifications and integrations
- **Export Capabilities**: Export data to Excel, CSV formats

---

## 📚 Related Documentation

### **Technical References**
- [Coinbase Advanced Trade API Documentation](https://docs.cloud.coinbase.com/advanced-trade-api/docs/welcome)
- [Implementation Roadmap](IMPLEMENTATION_ROADMAP.md)
- [Sandbox Features Documentation](../SANDBOX_FEATURES.md)

### **API Endpoints**
- [Advanced Trade Controller](../src/CoinbaseSandbox.Api/Controllers/AdvancedTradeController.cs)
- [Sandbox Controller](../src/CoinbaseSandbox.Api/Controllers/SandboxController.cs)
- [Technical Analysis Controller](../src/CoinbaseSandbox.Api/Controllers/TechnicalAnalysisController.cs)

---

This PRD provides a comprehensive blueprint for creating a powerful, user-friendly interface for the Coinbase Sandbox that will significantly improve the developer experience and make testing crypto trading applications much more efficient and enjoyable! 🚀 