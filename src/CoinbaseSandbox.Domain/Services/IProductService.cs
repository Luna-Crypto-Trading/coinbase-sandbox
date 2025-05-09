namespace CoinbaseSandbox.Domain.Services;

using CoinbaseSandbox.Domain.Models;

public interface IProductService
{
    Task<Product> GetProductAsync(string productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken = default);
}