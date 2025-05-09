namespace CoinbaseSandbox.Infrastructure.Repositories;

using System.Collections.Concurrent;
using Domain.Models;
using CoinbaseSandbox.Domain.Repositories;

public class InMemoryPriceRepository : IPriceRepository
{
    private readonly ConcurrentDictionary<string, List<PricePoint>> _prices = new();
    
    public Task<PricePoint?> GetLatestPriceAsync(string productId, CancellationToken cancellationToken = default)
    {
        if (_prices.TryGetValue(productId, out var pricePoints) && pricePoints.Count > 0)
        {
            return Task.FromResult<PricePoint?>(pricePoints.OrderByDescending(p => p.Timestamp).First());
        }
        
        return Task.FromResult<PricePoint?>(null);
    }
    
    public Task<IEnumerable<PricePoint>> GetPriceHistoryAsync(
        string productId, 
        DateTime start, 
        DateTime end, 
        CancellationToken cancellationToken = default)
    {
        if (_prices.TryGetValue(productId, out var pricePoints))
        {
            return Task.FromResult<IEnumerable<PricePoint>>(
                pricePoints
                    .Where(p => p.Timestamp >= start && p.Timestamp <= end)
                    .OrderBy(p => p.Timestamp)
                    .ToList());
        }
        
        return Task.FromResult<IEnumerable<PricePoint>>(Enumerable.Empty<PricePoint>());
    }
    
    public Task<PricePoint> AddAsync(PricePoint pricePoint, CancellationToken cancellationToken = default)
    {
        _prices.AddOrUpdate(
            pricePoint.ProductId,
            new List<PricePoint> { pricePoint },
            (_, existing) => 
            {
                existing.Add(pricePoint);
                return existing;
            });
            
        return Task.FromResult(pricePoint);
    }
}