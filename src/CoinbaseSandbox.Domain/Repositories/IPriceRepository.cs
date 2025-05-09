namespace CoinbaseSandbox.Domain.Repositories;

using Models;

public interface IPriceRepository
{
    Task<PricePoint?> GetLatestPriceAsync(string productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PricePoint>> GetPriceHistoryAsync(string productId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<PricePoint> AddAsync(PricePoint pricePoint, CancellationToken cancellationToken = default);
}