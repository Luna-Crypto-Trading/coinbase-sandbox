namespace CoinbaseSandbox.Domain.Services;

using Domain.Models;

public interface INotificationService
{
    Task SendOrderNotificationAsync(Order order, CancellationToken cancellationToken = default);
    Task SendPriceAlertAsync(string productId, decimal price, decimal percentChange, CancellationToken cancellationToken = default);
    Task SendBacktestCompletedNotificationAsync(string backtestId, string strategyName, decimal profitLossPercent, CancellationToken cancellationToken = default);
    Task SubscribeToPriceAlertsAsync(string productId, decimal threshold, CancellationToken cancellationToken = default);
    Task UnsubscribeFromPriceAlertsAsync(string productId, CancellationToken cancellationToken = default);
}