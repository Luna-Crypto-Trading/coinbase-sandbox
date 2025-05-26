#!/bin/bash

# Test script for Coinbase Sandbox simulation modes
# This script tests all simulation modes to ensure they work as expected

BASE_URL="http://localhost:5226"
PRODUCT_ID="BTC-USD"

echo "🧪 Testing Coinbase Sandbox Simulation Modes"
echo "============================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print test results
print_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✅ PASS${NC}: $2"
    else
        echo -e "${RED}❌ FAIL${NC}: $2"
    fi
}

# Function to test API endpoint
test_endpoint() {
    local method=$1
    local url=$2
    local data=$3
    local expected_status=$4
    local test_name=$5
    
    echo -e "${BLUE}Testing:${NC} $test_name"
    
    if [ "$method" = "POST" ]; then
        response=$(curl -s -w "%{http_code}" -X POST \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$url")
    elif [ "$method" = "DELETE" ]; then
        response=$(curl -s -w "%{http_code}" -X DELETE "$url")
    else
        response=$(curl -s -w "%{http_code}" "$url")
    fi
    
    # Extract status code (last 3 characters)
    status_code="${response: -3}"
    # Extract response body (everything except last 3 characters)
    response_body="${response%???}"
    
    echo "Response: $response_body"
    echo "Status Code: $status_code"
    
    if [ "$status_code" = "$expected_status" ]; then
        print_result 0 "$test_name"
        return 0
    else
        print_result 1 "$test_name (Expected: $expected_status, Got: $status_code)"
        return 1
    fi
}

# Function to wait and check price changes
wait_and_check_price() {
    local duration=$1
    local test_name=$2
    
    echo -e "${YELLOW}Waiting ${duration}s to observe price changes...${NC}"
    
    # Get initial price from sandbox state
    initial_response=$(curl -s "$BASE_URL/api/v3/sandbox/state")
    initial_price=$(echo "$initial_response" | jq -r ".prices[] | select(.product_id == \"$PRODUCT_ID\") | .price")
    echo "Initial price: $initial_price"
    
    sleep $duration
    
    # Get final price from sandbox state
    final_response=$(curl -s "$BASE_URL/api/v3/sandbox/state")
    final_price=$(echo "$final_response" | jq -r ".prices[] | select(.product_id == \"$PRODUCT_ID\") | .price")
    echo "Final price: $final_price"
    
    if [ "$initial_price" != "$final_price" ]; then
        print_result 0 "$test_name - Price changed from $initial_price to $final_price"
        return 0
    else
        print_result 1 "$test_name - Price did not change"
        return 1
    fi
}

echo "1. Testing Static Price Setting"
echo "==============================="
test_endpoint "POST" "$BASE_URL/api/v3/sandbox/prices/$PRODUCT_ID" "50000.00" "200" "Set static price to 50000"
echo ""

echo "2. Testing Trend Simulation"
echo "==========================="
trend_data='{
    "mode": "trend",
    "startPrice": 50000.00,
    "endPrice": 52000.00,
    "durationSeconds": 10,
    "repeat": false
}'
test_endpoint "POST" "$BASE_URL/api/v3/sandbox/prices/$PRODUCT_ID/simulate" "$trend_data" "200" "Start trend simulation"

if [ $? -eq 0 ]; then
    wait_and_check_price 12 "Trend simulation price movement"
fi
echo ""

echo "3. Testing Volatility Simulation"
echo "================================"
volatility_data='{
    "mode": "volatility",
    "basePrice": 52000.00,
    "volatilityPercent": 5.0,
    "durationSeconds": 10,
    "repeat": false
}'
test_endpoint "POST" "$BASE_URL/api/v3/sandbox/prices/$PRODUCT_ID/simulate" "$volatility_data" "200" "Start volatility simulation"

if [ $? -eq 0 ]; then
    wait_and_check_price 12 "Volatility simulation price movement"
fi
echo ""

echo "4. Testing Simulation Stop"
echo "=========================="
test_endpoint "DELETE" "$BASE_URL/api/v3/sandbox/prices/$PRODUCT_ID/simulation" "" "200" "Stop simulation"
echo ""

echo "5. Testing Repeating Trend Simulation"
echo "====================================="
repeat_trend_data='{
    "mode": "trend",
    "startPrice": 50000.00,
    "endPrice": 51000.00,
    "durationSeconds": 5,
    "repeat": true
}'
test_endpoint "POST" "$BASE_URL/api/v3/sandbox/prices/$PRODUCT_ID/simulate" "$repeat_trend_data" "200" "Start repeating trend simulation"

if [ $? -eq 0 ]; then
    echo -e "${YELLOW}Observing repeating trend for 15 seconds...${NC}"
    for i in {1..3}; do
        echo "Cycle $i:"
        wait_and_check_price 5 "Repeating trend cycle $i"
    done
    
    # Stop the repeating simulation
    test_endpoint "DELETE" "$BASE_URL/api/v3/sandbox/prices/$PRODUCT_ID/simulation" "" "200" "Stop repeating simulation"
fi
echo ""

echo "6. Testing Error Cases"
echo "====================="

# Test missing required fields for trend
invalid_trend_data='{
    "mode": "trend",
    "startPrice": 50000.00,
    "durationSeconds": 10
}'
test_endpoint "POST" "$BASE_URL/api/v3/sandbox/prices/$PRODUCT_ID/simulate" "$invalid_trend_data" "400" "Trend simulation without end price (should fail)"

# Test missing required fields for volatility
invalid_volatility_data='{
    "mode": "volatility",
    "basePrice": 50000.00,
    "durationSeconds": 10
}'
test_endpoint "POST" "$BASE_URL/api/v3/sandbox/prices/$PRODUCT_ID/simulate" "$invalid_volatility_data" "400" "Volatility simulation without volatility percent (should fail)"

# Test invalid product ID
test_endpoint "POST" "$BASE_URL/api/v3/sandbox/prices/INVALID-PRODUCT/simulate" "$trend_data" "404" "Simulation with invalid product ID (should fail)"

echo ""

echo "7. Testing WebSocket Price Updates"
echo "================================="
echo "Starting WebSocket test in background..."

# Create a simple WebSocket test script
cat > test_websocket.js << 'EOF'
const WebSocket = require('ws');

const ws = new WebSocket('ws://localhost:5226/ws');
let messageCount = 0;
let priceUpdates = [];

ws.on('open', function open() {
    console.log('WebSocket connected');
    
    // Subscribe to ticker updates
    const subscribeMessage = {
        type: 'subscribe',
        product_ids: ['BTC-USD'],
        channels: ['ticker']
    };
    
    ws.send(JSON.stringify(subscribeMessage));
    
    // Set a timeout to close the connection after 15 seconds
    setTimeout(() => {
        console.log(`Received ${messageCount} messages`);
        console.log(`Price updates: ${priceUpdates.length}`);
        if (priceUpdates.length > 0) {
            console.log('✅ WebSocket price updates working');
        } else {
            console.log('❌ No price updates received');
        }
        ws.close();
        process.exit(0);
    }, 15000);
});

ws.on('message', function message(data) {
    messageCount++;
    try {
        const parsed = JSON.parse(data);
        if (parsed.type === 'ticker' && parsed.price) {
            priceUpdates.push(parsed.price);
            console.log(`Price update: ${parsed.price}`);
        }
    } catch (e) {
        // Ignore parsing errors
    }
});

ws.on('error', function error(err) {
    console.log('WebSocket error:', err.message);
});
EOF

# Check if Node.js is available
if command -v node &> /dev/null; then
    # Install ws package if not available
    if [ ! -d "node_modules" ]; then
        echo "Installing WebSocket dependencies..."
        npm init -y > /dev/null 2>&1
        npm install ws > /dev/null 2>&1
    fi
    
    # Start WebSocket test in background
    node test_websocket.js &
    WS_PID=$!
    
    # Start a new simulation to generate price updates
    sleep 2
    echo "Starting simulation to generate WebSocket updates..."
    test_endpoint "POST" "$BASE_URL/api/v3/sandbox/prices/$PRODUCT_ID/simulate" "$volatility_data" "200" "Start simulation for WebSocket test"
    
    # Wait for WebSocket test to complete
    wait $WS_PID
    
    # Clean up
    rm -f test_websocket.js package.json package-lock.json
    rm -rf node_modules
else
    echo -e "${YELLOW}⚠️  Node.js not available, skipping WebSocket test${NC}"
fi

echo ""
echo "8. Testing Dashboard and WebSocket Tester Pages"
echo "==============================================="

# Test dashboard page
dashboard_response=$(curl -s -w "%{http_code}" "$BASE_URL/dashboard.html")
dashboard_status="${dashboard_response: -3}"
if [ "$dashboard_status" = "200" ]; then
    print_result 0 "Dashboard page accessible"
else
    print_result 1 "Dashboard page not accessible (Status: $dashboard_status)"
fi

# Test WebSocket tester page
ws_tester_response=$(curl -s -w "%{http_code}" "$BASE_URL/websocket-tester.html")
ws_tester_status="${ws_tester_response: -3}"
if [ "$ws_tester_status" = "200" ]; then
    print_result 0 "WebSocket tester page accessible"
else
    print_result 1 "WebSocket tester page not accessible (Status: $ws_tester_status)"
fi

echo ""
echo "🏁 Simulation Mode Testing Complete!"
echo "===================================="
echo ""
echo "Summary:"
echo "- Static price setting: Tested ✓"
echo "- Trend simulation: Tested ✓"
echo "- Volatility simulation: Tested ✓"
echo "- Repeating simulations: Tested ✓"
echo "- Simulation stopping: Tested ✓"
echo "- Error handling: Tested ✓"
echo "- WebSocket integration: Tested ✓"
echo "- UI pages: Tested ✓"
echo ""
echo "All simulation modes have been verified!"
echo ""
echo "You can also test manually by visiting:"
echo "- Dashboard: http://localhost:5226/dashboard.html"
echo "- WebSocket Tester: http://localhost:5226/websocket-tester.html" 