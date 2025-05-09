namespace CoinbaseSandbox.Infrastructure.External;

// This service would handle passthrough to the real Coinbase API
// For endpoints that don't involve wallet balances or trading
public class CoinbasePassthroughService
{
    private readonly ICoinbaseAdvancedTradeClient _client;
    
    public CoinbasePassthroughService(ICoinbaseAdvancedTradeClient client)
    {
        _client = client;
    }
    
    // This would implement the passthrough functionality
    // For now, it's just a placeholder
}