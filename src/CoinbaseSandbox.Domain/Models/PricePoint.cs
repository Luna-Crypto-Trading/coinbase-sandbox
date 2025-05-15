namespace CoinbaseSandbox.Domain.Models;

public record PricePoint
{
    public string ProductId { get; init; }
    public decimal Price { get; init; }
    public DateTime Timestamp { get; init; }

    public PricePoint(string productId, decimal price)
    {
        ProductId = productId;
        Price = price;
        Timestamp = DateTime.UtcNow;
    }
}