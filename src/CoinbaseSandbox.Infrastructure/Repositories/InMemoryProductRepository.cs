namespace CoinbaseSandbox.Infrastructure.Repositories;

using System.Collections.Concurrent;
using Domain.Models;
using CoinbaseSandbox.Domain.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<string, Product> _products = new();
    
    public Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }
    
    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Product>>(_products.Values.ToList());
    }
    
    public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products[product.Id] = product;
        return Task.FromResult(product);
    }
}