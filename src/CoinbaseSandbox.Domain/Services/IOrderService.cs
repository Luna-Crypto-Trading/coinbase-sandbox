namespace CoinbaseSandbox.Domain.Services;

using Models;

public interface IOrderService
{
    Task<Order> PlaceOrderAsync(string productId, OrderSide side, OrderType type, decimal size, decimal? limitPrice = null, CancellationToken cancellationToken = default);
    Task<Order> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOrdersAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<Order> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}