#!/bin/bash

# Test script to verify Coinbase API compatibility
BASE_URL="http://localhost:5226"

echo "🧪 Testing Coinbase API Compatibility"
echo "======================================"

# Test 1: Market Order (Real Coinbase API Format)
echo ""
echo "📊 Test 1: Market Order with Real API Format"
echo "---------------------------------------------"

MARKET_ORDER_PAYLOAD='{
  "client_order_id": "test-market-order-001",
  "product_id": "BTC-USD",
  "side": "BUY",
  "order_configuration": {
    "market_market_ioc": {
      "quote_size": "100.00"
    }
  }
}'

echo "Request payload:"
echo "$MARKET_ORDER_PAYLOAD" | jq .

echo ""
echo "Response:"
curl -s -X POST "$BASE_URL/api/v3/brokerage/orders" \
  -H "Content-Type: application/json" \
  -H "CB-ACCESS-KEY: test-key" \
  -H "CB-ACCESS-SIGN: test-sign" \
  -H "CB-ACCESS-TIMESTAMP: $(date +%s)" \
  -d "$MARKET_ORDER_PAYLOAD" | jq .

echo ""
echo "----------------------------------------"

# Test 2: Limit Order (Real Coinbase API Format)
echo ""
echo "📊 Test 2: Limit Order with Real API Format"
echo "--------------------------------------------"

LIMIT_ORDER_PAYLOAD='{
  "client_order_id": "test-limit-order-001",
  "product_id": "BTC-USD",
  "side": "BUY",
  "order_configuration": {
    "limit_limit_gtc": {
      "base_size": "0.001",
      "limit_price": "45000.00",
      "post_only": false
    }
  }
}'

echo "Request payload:"
echo "$LIMIT_ORDER_PAYLOAD" | jq .

echo ""
echo "Response:"
curl -s -X POST "$BASE_URL/api/v3/brokerage/orders" \
  -H "Content-Type: application/json" \
  -H "CB-ACCESS-KEY: test-key" \
  -H "CB-ACCESS-SIGN: test-sign" \
  -H "CB-ACCESS-TIMESTAMP: $(date +%s)" \
  -d "$LIMIT_ORDER_PAYLOAD" | jq .

echo ""
echo "----------------------------------------"

# Test 3: Stop Limit Order (Real Coinbase API Format)
echo ""
echo "📊 Test 3: Stop Limit Order with Real API Format"
echo "-------------------------------------------------"

STOP_LIMIT_PAYLOAD='{
  "client_order_id": "test-stop-limit-001",
  "product_id": "BTC-USD",
  "side": "SELL",
  "order_configuration": {
    "stop_limit_stop_limit_gtc": {
      "base_size": "0.001",
      "limit_price": "48000.00",
      "stop_price": "49000.00",
      "stop_direction": "STOP_DIRECTION_STOP_DOWN"
    }
  }
}'

echo "Request payload:"
echo "$STOP_LIMIT_PAYLOAD" | jq .

echo ""
echo "Response:"
curl -s -X POST "$BASE_URL/api/v3/brokerage/orders" \
  -H "Content-Type: application/json" \
  -H "CB-ACCESS-KEY: test-key" \
  -H "CB-ACCESS-SIGN: test-sign" \
  -H "CB-ACCESS-TIMESTAMP: $(date +%s)" \
  -d "$STOP_LIMIT_PAYLOAD" | jq .

echo ""
echo "----------------------------------------"

# Test 4: Invalid Request (Missing required fields)
echo ""
echo "📊 Test 4: Invalid Request (Missing client_order_id)"
echo "----------------------------------------------------"

INVALID_PAYLOAD='{
  "product_id": "BTC-USD",
  "side": "BUY",
  "order_configuration": {
    "market_market_ioc": {
      "quote_size": "100.00"
    }
  }
}'

echo "Request payload:"
echo "$INVALID_PAYLOAD" | jq .

echo ""
echo "Response:"
curl -s -X POST "$BASE_URL/api/v3/brokerage/orders" \
  -H "Content-Type: application/json" \
  -H "CB-ACCESS-KEY: test-key" \
  -H "CB-ACCESS-SIGN: test-sign" \
  -H "CB-ACCESS-TIMESTAMP: $(date +%s)" \
  -d "$INVALID_PAYLOAD" | jq .

echo ""
echo "✅ API Compatibility Tests Complete!"
echo "======================================" 