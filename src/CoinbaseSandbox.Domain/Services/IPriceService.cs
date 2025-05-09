namespace CoinbaseSandbox.Domain.Services;

using Models;

public interface IPriceService
{
    Task<decimal> GetCurrentPriceAsync(string productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PricePoint>> GetPriceHistoryAsync(string productId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<PricePoint> SetMockPriceAsync(string productId, decimal price, CancellationToken cancellationToken = default);
}