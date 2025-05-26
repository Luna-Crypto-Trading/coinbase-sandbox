using CoinbaseSandbox.Api.Models;
using CoinbaseSandbox.Domain.Models;

namespace CoinbaseSandbox.Api.Services;

public interface IOrderConfigurationParser
{
    (OrderType type, decimal size, decimal? limitPrice, string? timeInForce, DateTime? endTime) ParseOrderConfiguration(
        OrderConfiguration config, string side);
}

public class OrderConfigurationParser : IOrderConfigurationParser
{
    public (OrderType type, decimal size, decimal? limitPrice, string? timeInForce, DateTime? endTime) ParseOrderConfiguration(
        OrderConfiguration config, string side)
    {
        // Market orders
        if (config.MarketMarketIoc != null)
        {
            var size = GetOrderSize(config.MarketMarketIoc.BaseSize, config.MarketMarketIoc.QuoteSize, side);
            return (OrderType.Market, size, null, "IOC", null);
        }

        // Limit orders - Good Till Canceled
        if (config.LimitLimitGtc != null)
        {
            var size = GetOrderSize(config.LimitLimitGtc.BaseSize, config.LimitLimitGtc.QuoteSize, side);
            var limitPrice = decimal.Parse(config.LimitLimitGtc.LimitPrice);
            return (OrderType.Limit, size, limitPrice, "GTC", null);
        }

        // Limit orders - Good Till Date
        if (config.LimitLimitGtd != null)
        {
            var size = GetOrderSize(config.LimitLimitGtd.BaseSize, config.LimitLimitGtd.QuoteSize, side);
            var limitPrice = decimal.Parse(config.LimitLimitGtd.LimitPrice);
            var endTime = DateTime.Parse(config.LimitLimitGtd.EndTime);
            return (OrderType.Limit, size, limitPrice, "GTD", endTime);
        }

        // Limit orders - Fill or Kill
        if (config.LimitLimitFok != null)
        {
            var size = GetOrderSize(config.LimitLimitFok.BaseSize, config.LimitLimitFok.QuoteSize, side);
            var limitPrice = decimal.Parse(config.LimitLimitFok.LimitPrice);
            return (OrderType.Limit, size, limitPrice, "FOK", null);
        }

        // Smart Order Router Limit - Immediate or Cancel
        if (config.SorLimitIoc != null)
        {
            var size = GetOrderSize(config.SorLimitIoc.BaseSize, config.SorLimitIoc.QuoteSize, side);
            var limitPrice = decimal.Parse(config.SorLimitIoc.LimitPrice);
            return (OrderType.Limit, size, limitPrice, "IOC", null);
        }

        // Stop Limit orders - Good Till Canceled
        if (config.StopLimitStopLimitGtc != null)
        {
            var size = decimal.Parse(config.StopLimitStopLimitGtc.BaseSize);
            var limitPrice = decimal.Parse(config.StopLimitStopLimitGtc.LimitPrice);
            // For sandbox purposes, we'll treat stop-limit as regular limit orders
            return (OrderType.Limit, size, limitPrice, "GTC", null);
        }

        // Stop Limit orders - Good Till Date
        if (config.StopLimitStopLimitGtd != null)
        {
            var size = decimal.Parse(config.StopLimitStopLimitGtd.BaseSize);
            var limitPrice = decimal.Parse(config.StopLimitStopLimitGtd.LimitPrice);
            var endTime = DateTime.Parse(config.StopLimitStopLimitGtd.EndTime);
            return (OrderType.Limit, size, limitPrice, "GTD", endTime);
        }

        // Trigger Bracket orders - Good Till Canceled
        if (config.TriggerBracketGtc != null)
        {
            var size = decimal.Parse(config.TriggerBracketGtc.BaseSize);
            var limitPrice = decimal.Parse(config.TriggerBracketGtc.LimitPrice);
            return (OrderType.Limit, size, limitPrice, "GTC", null);
        }

        // Trigger Bracket orders - Good Till Date
        if (config.TriggerBracketGtd != null)
        {
            var size = decimal.Parse(config.TriggerBracketGtd.BaseSize);
            var limitPrice = decimal.Parse(config.TriggerBracketGtd.LimitPrice);
            var endTime = DateTime.Parse(config.TriggerBracketGtd.EndTime);
            return (OrderType.Limit, size, limitPrice, "GTD", endTime);
        }

        // TWAP orders - for sandbox, treat as limit orders
        if (config.TwapLimitGtd != null)
        {
            var size = GetOrderSize(config.TwapLimitGtd.BaseSize, config.TwapLimitGtd.QuoteSize, side);
            var limitPrice = decimal.Parse(config.TwapLimitGtd.LimitPrice);
            var endTime = DateTime.Parse(config.TwapLimitGtd.EndTime);
            return (OrderType.Limit, size, limitPrice, "GTD", endTime);
        }

        throw new ArgumentException("No valid order configuration provided");
    }

    private static decimal GetOrderSize(string? baseSize, string? quoteSize, string side)
    {
        if (!string.IsNullOrEmpty(baseSize))
        {
            return decimal.Parse(baseSize);
        }

        if (!string.IsNullOrEmpty(quoteSize))
        {
            // For quote size, we need to convert to base size
            // In a real implementation, you'd use current market price
            // For sandbox, we'll use a placeholder conversion
            var quoteSizeDecimal = decimal.Parse(quoteSize);

            // This is a simplified conversion - in reality you'd need current market price
            // For now, assume 1 unit of base = $50,000 (like BTC-USD)
            var estimatedPrice = 50000m;
            return quoteSizeDecimal / estimatedPrice;
        }

        throw new ArgumentException("Either base_size or quote_size must be provided");
    }
}