namespace CoinbaseSandbox.Domain.Models;

public record Product
{
    public string Id { get; init; } // Trading pair (e.g., "BTC-USD")
    public Currency BaseCurrency { get; init; } // The currency being bought/sold
    public Currency QuoteCurrency { get; init; } // The currency used to buy/sell
    public decimal MinimumOrderSize { get; init; }
    
    public Product(
        string id,
        Currency baseCurrency,
        Currency quoteCurrency,
        decimal minimumOrderSize)
    {
        Id = id;
        BaseCurrency = baseCurrency;
        QuoteCurrency = quoteCurrency;
        MinimumOrderSize = minimumOrderSize;
    }
}