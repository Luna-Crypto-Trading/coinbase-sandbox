namespace CoinbaseSandbox.Domain.Models;

public enum OrderSide
{
    Buy,
    Sell
}

public enum OrderType
{
    Market,
    Limit
}

public enum OrderStatus
{
    Pending,
    Open,
    Filled,
    Cancelled,
    Failed
}

public record Order
{
    public Guid Id { get; init; }
    public string ProductId { get; init; } // Trading pair (e.g., "BTC-USD")
    public OrderSide Side { get; init; }
    public OrderType Type { get; init; }
    public decimal Size { get; init; } // Amount of base currency
    public decimal? LimitPrice { get; init; } // Required for limit orders
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; private set; }
    public decimal? ExecutedPrice { get; private set; }
    public decimal? Fee { get; private set; }

    public Order(
        string productId,
        OrderSide side,
        OrderType type,
        decimal size,
        decimal? limitPrice = null)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Side = side;
        Type = type;
        Size = size;
        LimitPrice = limitPrice;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        ValidateOrder();
    }

    private void ValidateOrder()
    {
        if (string.IsNullOrWhiteSpace(ProductId))
            throw new ArgumentException("Product ID cannot be empty", nameof(ProductId));

        if (Size <= 0)
            throw new ArgumentException("Order size must be positive", nameof(Size));

        if (Type == OrderType.Limit && (!LimitPrice.HasValue || LimitPrice <= 0))
            throw new ArgumentException("Limit price must be positive for limit orders", nameof(LimitPrice));
    }

    public void Fill(decimal executedPrice, decimal fee)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Open)
            throw new InvalidOperationException($"Cannot fill order in {Status} status");

        Status = OrderStatus.Filled;
        ExecutedPrice = executedPrice;
        Fee = fee;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Open)
            throw new InvalidOperationException($"Cannot cancel order in {Status} status");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Open()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot open order in {Status} status");

        Status = OrderStatus.Open;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string reason)
    {
        Status = OrderStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}