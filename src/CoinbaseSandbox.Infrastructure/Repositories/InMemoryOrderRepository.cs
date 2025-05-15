namespace CoinbaseSandbox.Infrastructure.Repositories;

using System.Collections.Concurrent;
using Domain.Models;
using CoinbaseSandbox.Domain.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task<IEnumerable<Order>> GetAllAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Order>>(_orders.Values
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .ToList());
    }

    public Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _orders[order.Id] = order;
        return Task.FromResult(order);
    }

    public Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _orders[order.Id] = order;
        return Task.FromResult(order);
    }
}