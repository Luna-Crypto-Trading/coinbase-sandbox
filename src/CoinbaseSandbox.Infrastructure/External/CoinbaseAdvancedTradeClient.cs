namespace CoinbaseSandbox.Infrastructure.External;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

// This would be a real client for the Coinbase Advanced Trade API
// but for now we'll just define the interface and a mock implementation
public interface ICoinbaseAdvancedTradeClient
{
    Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    Task<T> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default);
    Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default);
}

public class MockCoinbaseAdvancedTradeClient : ICoinbaseAdvancedTradeClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public MockCoinbaseAdvancedTradeClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
    
    public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would make an actual API call
        // For now, we'll just throw a not implemented exception
        throw new NotImplementedException($"GET {endpoint} - This would call the actual Coinbase API");
    }
    
    public async Task<T> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would make an actual API call
        // For now, we'll just throw a not implemented exception
        throw new NotImplementedException($"POST {endpoint} - This would call the actual Coinbase API");
    }
    
    public async Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would make an actual API call
        // For now, we'll just throw a not implemented exception
        throw new NotImplementedException($"DELETE {endpoint} - This would call the actual Coinbase API");
    }
}