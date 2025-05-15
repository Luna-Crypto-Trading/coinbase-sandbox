namespace CoinbaseSandbox.Application.Services;

using System.Collections.Concurrent;
using CoinbaseSandbox.Domain.Models;
using CoinbaseSandbox.Domain.Services;
using Microsoft.Extensions.Logging;

public class TechnicalAnalysisService : ITechnicalAnalysisService
{
    private readonly IPriceService _priceService;
    private readonly ILogger<TechnicalAnalysisService> _logger;

    // Cache for technical indicators
    private readonly ConcurrentDictionary<string, Dictionary<string, decimal>> _indicatorCache = new();

    public TechnicalAnalysisService(
        IPriceService priceService,
        ILogger<TechnicalAnalysisService> logger)
    {
        _priceService = priceService;
        _logger = logger;
    }

    public async Task<Dictionary<string, decimal>> CalculateSimpleMovingAveragesAsync(
        string productId,
        int[] periods,
        CancellationToken cancellationToken = default)
    {
        // Get price history for the last 200 days (or the maximum period requested + buffer)
        int maxPeriod = periods.Max() + 10;
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-maxPeriod);

        var priceHistory = await _priceService.GetPriceHistoryAsync(
            productId,
            startDate,
            endDate,
            cancellationToken);

        var prices = priceHistory.OrderBy(p => p.Timestamp).ToList();

        if (!prices.Any())
        {
            throw new InvalidOperationException("Insufficient price data to calculate indicators");
        }

        var result = new Dictionary<string, decimal>();

        foreach (var period in periods)
        {
            // Ensure we have enough data
            if (prices.Count < period)
            {
                throw new InvalidOperationException($"Insufficient price data to calculate {period} period SMA");
            }

            // Calculate SMA
            var recentPrices = prices.Skip(Math.Max(0, prices.Count - period)).ToList();
            var sma = recentPrices.Average(p => p.Price);

            result[$"SMA{period}"] = sma;
        }

        return result;
    }

    public async Task<Dictionary<string, decimal>> CalculateExponentialMovingAveragesAsync(
        string productId,
        int[] periods,
        CancellationToken cancellationToken = default)
    {
        // Get price history for the last 200 days (or at least 3x the maximum period requested)
        int maxPeriod = periods.Max() * 3;
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-maxPeriod);

        var priceHistory = await _priceService.GetPriceHistoryAsync(
            productId,
            startDate,
            endDate,
            cancellationToken);

        var prices = priceHistory.OrderBy(p => p.Timestamp).ToList();

        if (!prices.Any())
        {
            throw new InvalidOperationException("Insufficient price data to calculate indicators");
        }

        var result = new Dictionary<string, decimal>();

        foreach (var period in periods)
        {
            // Ensure we have enough data
            if (prices.Count < period * 2)
            {
                throw new InvalidOperationException($"Insufficient price data to calculate {period} period EMA");
            }

            // Calculate EMA
            decimal multiplier = 2.0m / (period + 1);

            // Calculate the SMA for the first `period` data points
            decimal sma = prices.Take(period).Average(p => p.Price);
            decimal ema = sma;

            // Calculate the EMA for the remaining data points
            for (int i = period; i < prices.Count; i++)
            {
                ema = (prices[i].Price - ema) * multiplier + ema;
            }

            result[$"EMA{period}"] = ema;
        }

        return result;
    }

    public async Task<decimal> CalculateRelativeStrengthIndexAsync(
        string productId,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        // Get price history for the period + buffer
        int dataPoints = period * 3;
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-dataPoints);

        var priceHistory = await _priceService.GetPriceHistoryAsync(
            productId,
            startDate,
            endDate,
            cancellationToken);

        var prices = priceHistory.OrderBy(p => p.Timestamp).ToList();

        if (prices.Count < period + 1)
        {
            throw new InvalidOperationException($"Insufficient price data to calculate {period} period RSI");
        }

        // Calculate price changes
        var priceChanges = new List<decimal>();
        for (int i = 1; i < prices.Count; i++)
        {
            priceChanges.Add(prices[i].Price - prices[i - 1].Price);
        }

        // Calculate average gains and losses
        decimal avgGain = 0;
        decimal avgLoss = 0;

        // First period - simple average
        for (int i = 0; i < period; i++)
        {
            if (priceChanges[i] >= 0)
            {
                avgGain += priceChanges[i];
            }
            else
            {
                avgLoss += Math.Abs(priceChanges[i]);
            }
        }

        avgGain /= period;
        avgLoss /= period;

        // Rest of the periods - weighted average
        for (int i = period; i < priceChanges.Count; i++)
        {
            decimal change = priceChanges[i];

            if (change >= 0)
            {
                avgGain = ((avgGain * (period - 1)) + change) / period;
                avgLoss = (avgLoss * (period - 1)) / period;
            }
            else
            {
                avgGain = (avgGain * (period - 1)) / period;
                avgLoss = ((avgLoss * (period - 1)) + Math.Abs(change)) / period;
            }
        }

        // Calculate relative strength and RSI
        if (avgLoss == 0)
        {
            return 100; // No losses, RSI is 100
        }

        decimal rs = avgGain / avgLoss;
        decimal rsi = 100 - (100 / (1 + rs));

        return rsi;
    }

    public async Task<(decimal upper, decimal middle, decimal lower)> CalculateBollingerBandsAsync(
        string productId,
        int period = 20,
        double standardDeviations = 2.0,
        CancellationToken cancellationToken = default)
    {
        // Get price history for the period + buffer
        int dataPoints = period * 3;
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-dataPoints);

        var priceHistory = await _priceService.GetPriceHistoryAsync(
            productId,
            startDate,
            endDate,
            cancellationToken);

        var prices = priceHistory.OrderBy(p => p.Timestamp).ToList();

        if (prices.Count < period)
        {
            throw new InvalidOperationException($"Insufficient price data to calculate {period} period Bollinger Bands");
        }

        // Calculate the SMA (middle band)
        var recentPrices = prices.Skip(Math.Max(0, prices.Count - period)).ToList();
        decimal sma = recentPrices.Average(p => p.Price);

        // Calculate the standard deviation
        double sumSquaredDifferences = 0;
        foreach (var price in recentPrices)
        {
            double difference = (double)(price.Price - sma);
            sumSquaredDifferences += difference * difference;
        }

        double variance = sumSquaredDifferences / period;
        double standardDeviation = Math.Sqrt(variance);

        // Calculate the upper and lower bands
        decimal upperBand = sma + (decimal)(standardDeviation * standardDeviations);
        decimal lowerBand = sma - (decimal)(standardDeviation * standardDeviations);

        return (upperBand, sma, lowerBand);
    }

    public async Task<Dictionary<string, decimal>> GetTechnicalIndicatorsAsync(
        string productId,
        CancellationToken cancellationToken = default)
    {
        // Check if we have cached indicators
        string cacheKey = $"{productId}_{DateTime.UtcNow:yyyyMMddHHmm}"; // Cache for 1 minute

        if (_indicatorCache.TryGetValue(cacheKey, out var cachedIndicators))
        {
            return cachedIndicators;
        }

        // Calculate common indicators
        var indicators = new Dictionary<string, decimal>();

        try
        {
            // Get current price
            var currentPrice = await _priceService.GetCurrentPriceAsync(productId, cancellationToken);
            indicators["Price"] = currentPrice;

            // Calculate SMAs
            var smas = await CalculateSimpleMovingAveragesAsync(
                productId,
                new[] { 10, 20, 50, 200 },
                cancellationToken);

            foreach (var sma in smas)
            {
                indicators[sma.Key] = sma.Value;
            }

            // Calculate EMAs
            var emas = await CalculateExponentialMovingAveragesAsync(
                productId,
                new[] { 9, 12, 26 },
                cancellationToken);

            foreach (var ema in emas)
            {
                indicators[ema.Key] = ema.Value;
            }

            // Calculate RSI
            var rsi = await CalculateRelativeStrengthIndexAsync(
                productId,
                14,
                cancellationToken);

            indicators["RSI14"] = rsi;

            // Calculate Bollinger Bands
            var bollingerBands = await CalculateBollingerBandsAsync(
                productId,
                20,
                2.0,
                cancellationToken);

            indicators["BollingerUpper"] = bollingerBands.upper;
            indicators["BollingerMiddle"] = bollingerBands.middle;
            indicators["BollingerLower"] = bollingerBands.lower;

            // Calculate MACD
            decimal ema12 = emas["EMA12"];
            decimal ema26 = emas["EMA26"];
            decimal macd = ema12 - ema26;

            indicators["MACD"] = macd;

            // Calculate price relative to indicators (for trading signals)
            indicators["Price/SMA200%"] = (currentPrice / indicators["SMA200"]) * 100 - 100;
            indicators["Price/SMA50%"] = (currentPrice / indicators["SMA50"]) * 100 - 100;
            indicators["BollingerBandWidth%"] = ((bollingerBands.upper - bollingerBands.lower) / bollingerBands.middle) * 100;

            // Cache the results
            _indicatorCache[cacheKey] = indicators;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating technical indicators for {ProductId}", productId);
            throw;
        }

        return indicators;
    }
}