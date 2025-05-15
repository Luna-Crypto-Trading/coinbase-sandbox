namespace CoinbaseSandbox.Infrastructure.Services;

using System.Collections.Concurrent;
using CoinbaseSandbox.Domain.Models;
using CoinbaseSandbox.Domain.Services;
using Microsoft.Extensions.Logging;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly ConcurrentDictionary<string, decimal> _priceAlertThresholds = new();
    private readonly ConcurrentDictionary<string, decimal> _lastPrices = new();

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendOrderNotificationAsync(Order order, CancellationToken cancellationToken = default)
    {
        string status = order.Status switch
        {
            OrderStatus.Filled => "Filled",
            OrderStatus.Cancelled => "Cancelled",
            OrderStatus.Failed => "Failed",
            OrderStatus.Open => "Open",
            OrderStatus.Pending => "Pending",
            _ => "Unknown"
        };

        _logger.LogInformation(
            "Order Notification: {OrderId} - {Status} - {Side} {Size} {ProductId} @ {Price}",
            order.Id,
            status,
            order.Side,
            order.Size,
            order.ProductId,
            order.ExecutedPrice ?? order.LimitPrice);

        // In a real implementation, this would send an email, push notification, etc.

        return Task.CompletedTask;
    }

    public Task SendPriceAlertAsync(string productId, decimal price, decimal percentChange, CancellationToken cancellationToken = default)
    {
        string direction = percentChange >= 0 ? "up" : "down";
        decimal absChange = Math.Abs(percentChange);

        _logger.LogInformation(
            "Price Alert: {ProductId} {Direction} {PercentChange}% to {Price}",
            productId,
            direction,
            absChange,
            price);

        // In a real implementation, this would send an email, push notification, etc.

        return Task.CompletedTask;
    }

    public Task SendBacktestCompletedNotificationAsync(string backtestId, string strategyName, decimal profitLossPercent, CancellationToken cancellationToken = default)
    {
        string result = profitLossPercent >= 0 ? "Profit" : "Loss";

        _logger.LogInformation(
            "Backtest Completed: {BacktestId} - Strategy: {StrategyName} - Result: {Result} {ProfitLossPercent}%",
            backtestId,
            strategyName,
            result,
            Math.Abs(profitLossPercent));

        // In a real implementation, this would send an email, push notification, etc.

        return Task.CompletedTask;
    }

    public Task SubscribeToPriceAlertsAsync(string productId, decimal threshold, CancellationToken cancellationToken = default)
    {
        _priceAlertThresholds[productId] = threshold;

        _logger.LogInformation(
            "Subscribed to price alerts for {ProductId} with threshold {Threshold}%",
            productId,
            threshold);

        return Task.CompletedTask;
    }

    public Task UnsubscribeFromPriceAlertsAsync(string productId, CancellationToken cancellationToken = default)
    {
        _priceAlertThresholds.TryRemove(productId, out _);

        _logger.LogInformation(
            "Unsubscribed from price alerts for {ProductId}",
            productId);

        return Task.CompletedTask;
    }

    // This method would be called by the price service when prices change
    public async Task CheckPriceAlertsAsync(string productId, decimal price, CancellationToken cancellationToken = default)
    {
        if (!_priceAlertThresholds.TryGetValue(productId, out var threshold))
        {
            return; // No alert threshold set for this product
        }

        if (!_lastPrices.TryGetValue(productId, out var lastPrice))
        {
            // First time seeing this product, just record the price
            _lastPrices[productId] = price;
            return;
        }

        // Calculate percent change
        decimal percentChange = ((price - lastPrice) / lastPrice) * 100;

        // Check if the change exceeds the threshold
        if (Math.Abs(percentChange) >= threshold)
        {
            await SendPriceAlertAsync(productId, price, percentChange, cancellationToken);
        }

        // Update the last price
        _lastPrices[productId] = price;
    }
}