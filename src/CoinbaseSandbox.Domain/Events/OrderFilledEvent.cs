namespace CoinbaseSandbox.Domain.Events;

using Models;

public class OrderFilledEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid OrderId { get; }
    public string ProductId { get; }
    public OrderSide Side { get; }
    public decimal Size { get; }
    public decimal ExecutedPrice { get; }
    public decimal Fee { get; }

    public OrderFilledEvent(Order order)
    {
        if (order.Status != OrderStatus.Filled || !order.ExecutedPrice.HasValue || !order.Fee.HasValue)
            throw new ArgumentException("Order must be filled", nameof(order));

        OrderId = order.Id;
        ProductId = order.ProductId;
        Side = order.Side;
        Size = order.Size;
        ExecutedPrice = order.ExecutedPrice.Value;
        Fee = order.Fee.Value;
    }
}