using System.Text.Json.Serialization;

namespace CoinbaseSandbox.Api.Controllers;

using CoinbaseSandbox.Application.Dtos;
using CoinbaseSandbox.Domain.Models;
using CoinbaseSandbox.Domain.Services;
using CoinbaseSandbox.Infrastructure.External;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

/// <summary>
/// Controller that mimics the Coinbase Advanced Trade API routes exactly
/// This passes through product and pricing data from the real API
/// But mocks order execution, wallet operations, and account balances
/// </summary>
[ApiController]
[Route("api/v3/brokerage")]
public class AdvancedTradeController(
    IProductService productService,
    IOrderService orderService,
    IPriceService priceService,
    IWalletService walletService,
    CoinbaseApiClient coinbaseApiClient,
    ILogger<AdvancedTradeController> logger)
    : ControllerBase
{
    private readonly IProductService _productService = productService;

    #region Products (Passthrough to real API)

    public class BestBidAskData
    {
        [JsonPropertyName("product_id")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("bid")]
        public BidAskDetail Bid { get; set; } = new();

        [JsonPropertyName("ask")]
        public BidAskDetail Ask { get; set; } = new();
    }

    public class BidAskDetail
    {
        [JsonPropertyName("price")]
        public string Price { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public string Size { get; set; } = string.Empty;
    }

    public class BestBidAskResponse
    {
        [JsonPropertyName("pricebooks")]
        public List<BestBidAskData> Pricebooks { get; set; } = new();

        [JsonPropertyName("success")]
        public bool Success { get; set; } = true;
    }

    
    [HttpGet("best_bid_ask")]
    public async Task<IActionResult> GetBestBidAsk(
        [FromQuery(Name = "product_ids")] List<string>? productIds = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get authentication headers from request
            if (!TryGetAuthHeaders(out var accessKey, out var accessSign, out var accessTimestamp))
            {
                return BadRequest(new { error = "Missing or invalid authentication headers" });
            }

            // If product IDs are provided, we'll pass them through to the real API
            if (productIds != null && productIds.Any())
            {
                try
                {
                    // Construct the proper query string with multiple product_ids
                    string queryString = string.Join("&", productIds.Select(id => $"product_ids={Uri.EscapeDataString(id)}"));
                    string endpoint = $"/api/v3/brokerage/best_bid_ask?{queryString}";

                    // Pass through to real Coinbase API
                    var bestBidAskData = await coinbaseApiClient.GetAsync<JsonElement>(
                        endpoint,
                        accessKey,
                        accessSign,
                        accessTimestamp,
                        cancellationToken);

                    return Ok(bestBidAskData);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error fetching best bid/ask data from Coinbase API");

                    // Fall back to mock data if real API fails
                    return await GetMockBestBidAskAsync(productIds, cancellationToken);
                }
            }
            else
            {
                // If no product IDs specified, return data for all available products
                var products = await _productService.GetProductsAsync(cancellationToken);
                productIds = products.Select(p => p.Id).ToList();

                return await GetMockBestBidAskAsync(productIds, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetBestBidAsk");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    
    // GET /api/v3/brokerage/products
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        try
        {
            // Get authentication headers from request
            if (!TryGetAuthHeaders(out var accessKey, out var accessSign, out var accessTimestamp))
            {
                return BadRequest(new { error = "Missing or invalid authentication headers" });
            }

            // Pass through to real Coinbase API
            var productsData = await coinbaseApiClient.GetProductsAsync<JsonElement>(
                accessKey,
                accessSign,
                accessTimestamp,
                cancellationToken);

            return Ok(productsData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching products from Coinbase API");
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = "Error fetching products from Coinbase API",
                message = ex.Message
            });
        }
    }

    // GET /api/v3/brokerage/products/{product_id}
    [HttpGet("products/{productId}")]
    public async Task<IActionResult> GetProduct(string productId, CancellationToken cancellationToken)
    {
        try
        {
            // Get authentication headers from request
            if (!TryGetAuthHeaders(out var accessKey, out var accessSign, out var accessTimestamp))
            {
                return BadRequest(new { error = "Missing or invalid authentication headers" });
            }

            // Pass through to real Coinbase API
            var productData = await coinbaseApiClient.GetProductAsync<JsonElement>(
                productId,
                accessKey,
                accessSign,
                accessTimestamp,
                cancellationToken);

            return Ok(productData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching product {ProductId} from Coinbase API", productId);
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = $"Error fetching product {productId} from Coinbase API",
                message = ex.Message
            });
        }
    }

    // GET /api/v3/brokerage/products/{product_id}/candles
    [HttpGet("products/{productId}/candles")]
    public async Task<IActionResult> GetProductCandles(
        string productId,
        [FromQuery] string start,
        [FromQuery] string end,
        [FromQuery] string granularity,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get authentication headers from request
            if (!TryGetAuthHeaders(out var accessKey, out var accessSign, out var accessTimestamp))
            {
                return BadRequest(new { error = "Missing or invalid authentication headers" });
            }

            // Pass through to real Coinbase API
            var candlesData = await coinbaseApiClient.GetProductCandlesAsync<JsonElement>(
                productId,
                start,
                end,
                granularity,
                accessKey,
                accessSign,
                accessTimestamp,
                cancellationToken);

            return Ok(candlesData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching candles for {ProductId} from Coinbase API", productId);
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = $"Error fetching candles for {productId} from Coinbase API",
                message = ex.Message
            });
        }
    }

    // GET /api/v3/brokerage/products/{product_id}/ticker
    [HttpGet("products/{productId}/ticker")]
    public async Task<IActionResult> GetProductTicker(string productId, CancellationToken cancellationToken)
    {
        try
        {
            // Get authentication headers from request
            if (!TryGetAuthHeaders(out var accessKey, out var accessSign, out var accessTimestamp))
            {
                return BadRequest(new { error = "Missing or invalid authentication headers" });
            }

            // Pass through to real Coinbase API
            var tickerData = await coinbaseApiClient.GetMarketTradesAsync<JsonElement>(
                productId,
                accessKey,
                accessSign,
                accessTimestamp,
                cancellationToken);

            return Ok(tickerData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching ticker for {ProductId} from Coinbase API", productId);
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = $"Error fetching ticker for {productId} from Coinbase API",
                message = ex.Message
            });
        }
    }

    // GET /api/v3/brokerage/products/{product_id}/book
    [HttpGet("products/{productId}/book")]
    public async Task<IActionResult> GetProductBook(string productId, CancellationToken cancellationToken)
    {
        try
        {
            // Get authentication headers from request
            if (!TryGetAuthHeaders(out var accessKey, out var accessSign, out var accessTimestamp))
            {
                return BadRequest(new { error = "Missing or invalid authentication headers" });
            }

            // Pass through to real Coinbase API
            var bookData = await coinbaseApiClient.GetProductBookAsync<JsonElement>(
                productId,
                accessKey,
                accessSign,
                accessTimestamp,
                cancellationToken);

            return Ok(bookData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching order book for {ProductId} from Coinbase API", productId);
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = $"Error fetching order book for {productId} from Coinbase API",
                message = ex.Message
            });
        }
    }

    #endregion

    #region Orders (Mocked locally)

    // POST /api/v3/brokerage/orders
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Parse side
            if (!Enum.TryParse<OrderSide>(request.side, true, out var side))
            {
                return BadRequest(new { error = $"Invalid order side: {request.side}" });
            }

            // Parse type
            if (!Enum.TryParse<OrderType>(request.order_type, true, out var type))
            {
                return BadRequest(new { error = $"Invalid order type: {request.order_type}" });
            }

            // Validate limit price for limit orders
            decimal? limitPrice = null;
            if (type == OrderType.Limit)
            {
                if (string.IsNullOrEmpty(request.limit_price))
                {
                    return BadRequest(new { error = "Limit price is required for limit orders" });
                }

                if (!decimal.TryParse(request.limit_price, out var parsedLimitPrice))
                {
                    return BadRequest(new { error = $"Invalid limit price: {request.limit_price}" });
                }

                limitPrice = parsedLimitPrice;
            }

            // Parse size
            if (!decimal.TryParse(request.base_size, out var size))
            {
                return BadRequest(new { error = $"Invalid base size: {request.base_size}" });
            }

            // Attempt to get current market price from real API for the product
            decimal executionPrice = 0;
            try
            {
                if (TryGetAuthHeaders(out var accessKey, out var accessSign, out var accessTimestamp))
                {
                    var tickerData = await coinbaseApiClient.GetMarketTradesAsync<JsonElement>(
                        request.product_id,
                        accessKey,
                        accessSign,
                        accessTimestamp,
                        cancellationToken);

                    if (tickerData.TryGetProperty("trades", out var trades) &&
                        trades.GetArrayLength() > 0 &&
                        trades[0].TryGetProperty("price", out var priceElement) &&
                        decimal.TryParse(priceElement.GetString(), out var price))
                    {
                        executionPrice = price;

                        // Update our local price record with the current market price
                        await priceService.SetMockPriceAsync(request.product_id, executionPrice, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error fetching current price for {ProductId} from real API, using local price", request.product_id);
            }

            // Fall back to local price if we couldn't get it from the real API
            if (executionPrice == 0)
            {
                try
                {
                    executionPrice = await priceService.GetCurrentPriceAsync(request.product_id, cancellationToken);
                }
                catch
                {
                    // If no price is available locally, use a mock price
                    executionPrice = 50000.00m; // Default price for testing
                    await priceService.SetMockPriceAsync(request.product_id, executionPrice, cancellationToken);
                }
            }

            // Place the order in our local system
            var order = await orderService.PlaceOrderAsync(
                request.product_id,
                side,
                type,
                size,
                limitPrice,
                cancellationToken);

            // Format response to match Coinbase API format
            var response = new
            {
                success = true,
                order_id = order.Id.ToString(),
                order_configuration = new
                {
                    order_id = order.Id.ToString(),
                    product_id = order.ProductId,
                    side = order.Side.ToString().ToLower(),
                    order_type = order.Type.ToString().ToLower(),
                    status = order.Status.ToString().ToLower(),
                    created_time = order.CreatedAt.ToString("o"),
                    base_size = order.Size.ToString(),
                    limit_price = order.LimitPrice?.ToString(),
                    filled_size = order.Status == OrderStatus.Filled ? order.Size.ToString() : "0",
                    filled_price = order.ExecutedPrice?.ToString(),
                    fee = order.Fee?.ToString()
                }
            };

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public class CreateOrderRequest
    {
        public string client_order_id { get; set; } = Guid.NewGuid().ToString();
        public string product_id { get; set; } = string.Empty;
        public string side { get; set; } = string.Empty;
        public string order_type { get; set; } = string.Empty;
        public string base_size { get; set; } = string.Empty;
        public string limit_price { get; set; } = string.Empty;
    }

    // GET /api/v3/brokerage/orders/historical/{order_id}
    [HttpGet("orders/historical/{orderId}")]
    public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.GetOrderAsync(orderId, cancellationToken);

            // Format response to match Coinbase API format
            var response = new
            {
                order = new
                {
                    order_id = order.Id.ToString(),
                    product_id = order.ProductId,
                    side = order.Side.ToString().ToLower(),
                    order_type = order.Type.ToString().ToLower(),
                    status = order.Status.ToString().ToLower(),
                    created_time = order.CreatedAt.ToString("o"),
                    base_size = order.Size.ToString(),
                    limit_price = order.LimitPrice?.ToString(),
                    filled_size = order.Status == OrderStatus.Filled ? order.Size.ToString() : "0",
                    filled_price = order.ExecutedPrice?.ToString(),
                    fee = order.Fee?.ToString()
                }
            };

            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Order {orderId} not found" });
        }
    }

    // GET /api/v3/brokerage/orders/historical
    [HttpGet("orders/historical")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var orders = await orderService.GetOrdersAsync(limit, cancellationToken);

        // Format response to match Coinbase API format
        var response = new
        {
            orders = orders.Select(order => new
            {
                order_id = order.Id.ToString(),
                product_id = order.ProductId,
                side = order.Side.ToString().ToLower(),
                order_type = order.Type.ToString().ToLower(),
                status = order.Status.ToString().ToLower(),
                created_time = order.CreatedAt.ToString("o"),
                base_size = order.Size.ToString(),
                limit_price = order.LimitPrice?.ToString(),
                filled_size = order.Status == OrderStatus.Filled ? order.Size.ToString() : "0",
                filled_price = order.ExecutedPrice?.ToString(),
                fee = order.Fee?.ToString()
            }).ToList(),
            has_next = false
        };

        return Ok(response);
    }

    // POST /api/v3/brokerage/orders/batch_cancel
    [HttpPost("orders/batch_cancel")]
    public async Task<IActionResult> CancelOrders([FromBody] CancelOrdersRequest request, CancellationToken cancellationToken)
    {
        if (request.order_ids == null || !request.order_ids.Any())
        {
            return BadRequest(new { error = "No order IDs provided" });
        }

        var results = new List<object>();

        foreach (var orderIdStr in request.order_ids)
        {
            if (!Guid.TryParse(orderIdStr, out var orderId))
            {
                results.Add(new
                {
                    order_id = orderIdStr,
                    success = false,
                    failure_reason = "Invalid order ID format"
                });
                continue;
            }

            try
            {
                var order = await orderService.CancelOrderAsync(orderId, cancellationToken);

                results.Add(new
                {
                    order_id = orderIdStr,
                    success = true
                });
            }
            catch (KeyNotFoundException)
            {
                results.Add(new
                {
                    order_id = orderIdStr,
                    success = false,
                    failure_reason = "Order not found"
                });
            }
            catch (InvalidOperationException ex)
            {
                results.Add(new
                {
                    order_id = orderIdStr,
                    success = false,
                    failure_reason = ex.Message
                });
            }
        }

        var response = new { results };
        return Ok(response);
    }

    public class CancelOrdersRequest
    {
        public List<string> order_ids { get; set; } = new List<string>();
    }

    #endregion
    private async Task<IActionResult> GetMockBestBidAskAsync(
        List<string> productIds,
        CancellationToken cancellationToken)
    {
        var response = new BestBidAskResponse
        {
            Pricebooks = new List<BestBidAskData>(),
            Success = true
        };

        foreach (var productId in productIds)
        {
            try
            {
                // Get current price for the product
                var currentPrice = await priceService.GetCurrentPriceAsync(productId, cancellationToken);

                // Create simulated bid and ask prices (bid slightly lower, ask slightly higher)
                decimal bidPrice = currentPrice * 0.999m; // 0.1% below current price
                decimal askPrice = currentPrice * 1.001m; // 0.1% above current price

                // Generate random sizes
                var random = new Random();
                decimal bidSize = Math.Round(0.1m + (decimal)random.NextDouble() * 2, 6);
                decimal askSize = Math.Round(0.1m + (decimal)random.NextDouble() * 2, 6);

                response.Pricebooks.Add(new BestBidAskData
                {
                    ProductId = productId,
                    Bid = new BidAskDetail
                    {
                        Price = bidPrice.ToString("F2"),
                        Size = bidSize.ToString("F6")
                    },
                    Ask = new BidAskDetail
                    {
                        Price = askPrice.ToString("F2"),
                        Size = askSize.ToString("F6")
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error getting mock best bid/ask for {ProductId}", productId);
                // Skip this product if there's an error
            }
        }

        return Ok(response);
    }

    #region Accounts (Mocked locally)

    // GET /api/v3/brokerage/accounts
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        var wallets = await walletService.GetWalletsAsync(cancellationToken);

        // In this sandbox, we'll treat each wallet as an account
        // Format response to match Coinbase API format
        var response = new
        {
            accounts = wallets.SelectMany(w => w.Assets.Select(a => new
            {
                uuid = Guid.NewGuid().ToString(), // Generate a random UUID for each asset
                name = $"{a.Currency.Symbol} Wallet",
                currency = a.Currency.Symbol,
                available_balance = new { value = a.Balance.ToString(), currency = a.Currency.Symbol },
                isDefault = true,
                active = true,
                created_at = DateTime.UtcNow.AddDays(-30).ToString("o"), // Mock creation date
                updated_at = DateTime.UtcNow.ToString("o"),
                deleted_at = (string)null,
                type = "ACCOUNT_TYPE_CRYPTO"
            })).ToList(),
            has_next = false,
            cursor = ""
        };

        return Ok(response);
    }

    #endregion

    #region Sandbox-Specific Endpoints

    // This is a sandbox-specific endpoint for setting mock prices manually
    [HttpPost("sandbox/prices/{productId}")]
    public async Task<IActionResult> SetMockPrice(
        string productId,
        [FromBody] decimal price,
        CancellationToken cancellationToken)
    {
        try
        {
            var pricePoint = await priceService.SetMockPriceAsync(
                productId,
                price,
                cancellationToken);

            return Ok(new
            {
                success = true,
                product_id = pricePoint.ProductId,
                price = pricePoint.Price.ToString(),
                timestamp = pricePoint.Timestamp.ToString("o")
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Tries to extract Coinbase authentication headers from the request,
    /// including support for both CB-ACCESS headers and Bearer token
    /// </summary>
    private bool TryGetAuthHeaders(out string accessKey, out string accessSign, out string accessTimestamp)
    {
        accessKey = string.Empty;
        accessSign = string.Empty;
        accessTimestamp = string.Empty;

        // Try the traditional CB-ACCESS headers first
        if (Request.Headers.TryGetValue("CB-ACCESS-KEY", out var keyValues))
            accessKey = keyValues.ToString();

        if (Request.Headers.TryGetValue("CB-ACCESS-SIGN", out var signValues))
            accessSign = signValues.ToString();

        if (Request.Headers.TryGetValue("CB-ACCESS-TIMESTAMP", out var timestampValues))
            accessTimestamp = timestampValues.ToString();

        // If traditional headers aren't present, try extract from Bearer token
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(accessSign) || string.IsNullOrEmpty(accessTimestamp))
        {
            if (Request.Headers.TryGetValue("Authorization", out var authHeader) &&
                authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.ToString().Substring("Bearer ".Length).Trim();

                try
                {
                    // Parse the JWT parts (header, payload, signature)
                    var parts = token.Split('.');
                    if (parts.Length >= 2)
                    {
                        // For simplicity, we'll extract the token itself as our key
                        // and derive sign and timestamp from it
                        accessKey = token;
                        accessSign = parts[2]; // The signature part
                        accessTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

                        logger.LogInformation("Using Bearer token authentication");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to parse Bearer token");
                    return false;
                }
            }
        }

        return !string.IsNullOrEmpty(accessKey) &&
               !string.IsNullOrEmpty(accessSign) &&
               !string.IsNullOrEmpty(accessTimestamp);
    }
    #endregion
}