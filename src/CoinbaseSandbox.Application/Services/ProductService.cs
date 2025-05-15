namespace CoinbaseSandbox.Application.Services;

using Domain.Models;
using Domain.Repositories;
using CoinbaseSandbox.Domain.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Product> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"Product {productId} not found");

        return product;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetAllAsync(cancellationToken);
    }
}