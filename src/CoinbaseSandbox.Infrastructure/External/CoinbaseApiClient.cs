namespace CoinbaseSandbox.Infrastructure.External;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

/// <summary>
/// Client for making requests to the real Coinbase API using the authentication 
/// from the original request.
/// </summary>
public class CoinbaseApiClient(HttpClient httpClient, ILogger<CoinbaseApiClient> logger)
{
    /// <summary>
    /// Passes through a GET request to the real Coinbase API using the provided headers
    /// </summary>
    public async Task<T?> GetAsync<T>(
        string endpoint,
        string cbAccessKey,
        string cbAccessSign,
        string cbAccessTimestamp,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create request with authentication headers from original request
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            // Check if authentication is via JWT Bearer token
            if (cbAccessKey.Contains('.') && cbAccessKey.Split('.').Length >= 2)
            {
                // Forward the bearer token
                request.Headers.Add("Authorization", $"Bearer {cbAccessKey}");
                logger.LogInformation("Using Bearer token authentication for passthrough");
            }
            else
            {
                // Using traditional headers
                request.Headers.Add("CB-ACCESS-KEY", cbAccessKey);
                request.Headers.Add("CB-ACCESS-SIGN", cbAccessSign);
                request.Headers.Add("CB-ACCESS-TIMESTAMP", cbAccessTimestamp);
            }

            // Send request to real Coinbase API
            var response = await httpClient.SendAsync(request, cancellationToken);

            // Ensure we got a successful response
            response.EnsureSuccessStatusCode();

            // Parse and return the response
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null, // Keep original property names
                PropertyNameCaseInsensitive = true // Allow case-insensitive matching
            };
            
            return JsonSerializer.Deserialize<T>(content, options);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error passing through GET request to Coinbase API: {Endpoint}", endpoint);
            throw;
        }
    }
    
    /// <summary>
    /// Fetches best bid and ask data from the real Coinbase API
    /// </summary>
    public async Task<T?> GetBestBidAskAsync<T>(
        List<string> productIds,
        string cbAccessKey,
        string cbAccessSign,
        string cbAccessTimestamp,
        CancellationToken cancellationToken = default)
    {
        // Construct the query string with multiple product_ids
        string queryString = string.Join("&", productIds.Select(id => $"product_ids={Uri.EscapeDataString(id)}"));
        string endpoint = $"/api/v3/brokerage/best_bid_ask?{queryString}";
    
        return await GetAsync<T>(
            endpoint,
            cbAccessKey,
            cbAccessSign,
            cbAccessTimestamp,
            cancellationToken);
    }
    
    /// <summary>
    /// Fetches product data from the real Coinbase API
    /// </summary>
    public async Task<T?> GetProductsAsync<T>(
        string cbAccessKey,
        string cbAccessSign,
        string cbAccessTimestamp,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<T>(
            "/api/v3/brokerage/products",
            cbAccessKey,
            cbAccessSign,
            cbAccessTimestamp,
            cancellationToken);
    }

    /// <summary>
    /// Fetches specific product data from the real Coinbase API
    /// </summary>
    public async Task<T?> GetProductAsync<T>(
        string productId,
        string cbAccessKey,
        string cbAccessSign,
        string cbAccessTimestamp,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<T>(
            $"/api/v3/brokerage/products/{productId}",
            cbAccessKey,
            cbAccessSign,
            cbAccessTimestamp,
            cancellationToken);
    }

    /// <summary>
    /// Fetches product candles (price history) from the real Coinbase API
    /// </summary>
    public async Task<T?> GetProductCandlesAsync<T>(
        string productId,
        string start,
        string end,
        string granularity,
        string cbAccessKey,
        string cbAccessSign,
        string cbAccessTimestamp,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<T>(
            $"/api/v3/brokerage/products/{productId}/candles?start={start}&end={end}&granularity={granularity}",
            cbAccessKey,
            cbAccessSign,
            cbAccessTimestamp,
            cancellationToken);
    }

    /// <summary>
    /// Fetches market trades from the real Coinbase API
    /// </summary>
    public async Task<T?> GetMarketTradesAsync<T>(
        string productId,
        string cbAccessKey,
        string cbAccessSign,
        string cbAccessTimestamp,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<T>(
            $"/api/v3/brokerage/products/{productId}/ticker",
            cbAccessKey,
            cbAccessSign,
            cbAccessTimestamp,
            cancellationToken);
    }

    /// <summary>
    /// Fetches best bid/ask from the real Coinbase API
    /// </summary>
    public async Task<T?> GetProductBookAsync<T>(
        string productId,
        string cbAccessKey,
        string cbAccessSign,
        string cbAccessTimestamp,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<T>(
            $"/api/v3/brokerage/products/{productId}/book",
            cbAccessKey,
            cbAccessSign,
            cbAccessTimestamp,
            cancellationToken);
    }
}