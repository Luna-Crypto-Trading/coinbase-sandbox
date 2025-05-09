namespace CoinbaseSandbox.Application.Services;

using Domain.Models;
using Domain.Repositories;
using CoinbaseSandbox.Domain.Services;

public class PriceService : IPriceService
{
    private readonly IPriceRepository _priceRepository;
    private readonly IProductRepository _productRepository;
    
    public PriceService(
        IPriceRepository priceRepository,
        IProductRepository productRepository)
    {
        _priceRepository = priceRepository;
        _productRepository = productRepository;
    }
    
    public async Task<decimal> GetCurrentPriceAsync(string productId, CancellationToken cancellationToken = default)
    {
        // Check if product exists
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
            throw new ArgumentException($"Product {productId} not found", nameof(productId));
            
        // Get latest price point
        var pricePoint = await _priceRepository.GetLatestPriceAsync(productId, cancellationToken);
        if (pricePoint == null)
            throw new InvalidOperationException($"No price data available for {productId}");
            
        return pricePoint.Price;
    }
    
    public async Task<IEnumerable<PricePoint>> GetPriceHistoryAsync(
        string productId, 
        DateTime start, 
        DateTime end, 
        CancellationToken cancellationToken = default)
    {
        // Check if product exists
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
            throw new ArgumentException($"Product {productId} not found", nameof(productId));
            
        return await _priceRepository.GetPriceHistoryAsync(productId, start, end, cancellationToken);
    }
    
    public async Task<PricePoint> SetMockPriceAsync(
        string productId, 
        decimal price, 
        CancellationToken cancellationToken = default)
    {
        // Check if product exists
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
            throw new ArgumentException($"Product {productId} not found", nameof(productId));
            
        // Create and store the price point
        var pricePoint = new PricePoint(productId, price);
        return await _priceRepository.AddAsync(pricePoint, cancellationToken);
    }
}