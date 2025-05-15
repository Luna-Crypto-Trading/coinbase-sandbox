namespace CoinbaseSandbox.Application.Services;

using System.Collections.Concurrent;
using System.Text.Json;
using CoinbaseSandbox.Domain.Models;
using CoinbaseSandbox.Domain.Services;
using Microsoft.Extensions.Logging;

public class BacktestService : IBacktestService
{
    private readonly IPriceService _priceService;
    private readonly IProductService _productService;
    private readonly ILogger<BacktestService> _logger;

    // In-memory storage for backtest results and strategies
    private readonly ConcurrentDictionary<string, BacktestResult> _results = new();
    private readonly ConcurrentDictionary<string, BacktestStrategy> _strategies = new();

    public BacktestService(
        IPriceService priceService,
        IProductService productService,
        ILogger<BacktestService> logger)
    {
        _priceService = priceService;
        _productService = productService;
        _logger = logger;

        // Add some example strategies
        InitializeExampleStrategies();
    }

    private void InitializeExampleStrategies()
    {
        // Simple Moving Average Crossover Strategy
        var smaCrossover = new BacktestStrategy
        {
            Name = "SMA Crossover",
            Description = "Simple moving average crossover strategy that buys when short SMA crosses above long SMA and sells when it crosses below",
            Parameters = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                { "ShortPeriod", "10" },
                { "LongPeriod", "30" }
            }),
            Code = @"
// Simple Moving Average Crossover Strategy
// Buy when short SMA crosses above long SMA
// Sell when short SMA crosses below long SMA

var shortPeriod = int.Parse(parameters[""ShortPeriod""]);
var longPeriod = int.Parse(parameters[""LongPeriod""]);

// Calculate SMAs
var shortSma = CalculateSMA(prices, shortPeriod);
var longSma = CalculateSMA(prices, longPeriod);

// Trading logic
bool lastCrossed = false;
for (int i = longPeriod; i < prices.Count; i++)
{
    bool isCrossedAbove = shortSma[i] > longSma[i];
    
    if (isCrossedAbove && !lastCrossed && cryptoBalance == 0)
    {
        // Buy signal - short SMA crossed above long SMA
        decimal amountToSpend = cashBalance * 0.98m; // 98% of cash, leaving some for fees
        decimal size = amountToSpend / prices[i].Price;
        
        ExecuteTrade(OrderSide.Buy, prices[i].Timestamp, prices[i].Price, size, ""SMA Crossed Above"");
    }
    else if (!isCrossedAbove && lastCrossed && cryptoBalance > 0)
    {
        // Sell signal - short SMA crossed below long SMA
        ExecuteTrade(OrderSide.Sell, prices[i].Timestamp, prices[i].Price, cryptoBalance, ""SMA Crossed Below"");
    }
    
    lastCrossed = isCrossedAbove;
}

// Helper method to calculate SMA
List<decimal> CalculateSMA(List<PricePoint> prices, int period)
{
    var result = new List<decimal>();
    
    // Initialize with zeros for the first period-1 elements
    for (int i = 0; i < period - 1; i++)
    {
        result.Add(0);
    }
    
    // Calculate SMA for the rest
    for (int i = period - 1; i < prices.Count; i++)
    {
        decimal sum = 0;
        for (int j = i - period + 1; j <= i; j++)
        {
            sum += prices[j].Price;
        }
        result.Add(sum / period);
    }
    
    return result;
}"
        };

        // Bollinger Bands Strategy
        var bollingerBands = new BacktestStrategy
        {
            Name = "Bollinger Bands",
            Description = "Buys when price touches the lower band and sells when it touches the upper band",
            Parameters = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                { "Period", "20" },
                { "StandardDeviations", "2" }
            }),
            Code = @"
// Bollinger Bands Strategy
// Buy when price touches the lower band
// Sell when price touches the upper band

var period = int.Parse(parameters[""Period""]);
var stdDevMultiplier = double.Parse(parameters[""StandardDeviations""]);

for (int i = period; i < prices.Count; i++)
{
    // Calculate SMA
    decimal sum = 0;
    for (int j = i - period; j < i; j++)
    {
        sum += prices[j].Price;
    }
    decimal sma = sum / period;
    
    // Calculate standard deviation
    double variance = 0;
    for (int j = i - period; j < i; j++)
    {
        variance += Math.Pow((double)(prices[j].Price - sma), 2);
    }
    double stdDev = Math.Sqrt(variance / period);
    
    // Calculate bands
    decimal upperBand = sma + (decimal)(stdDev * stdDevMultiplier);
    decimal lowerBand = sma - (decimal)(stdDev * stdDevMultiplier);
    
    // Trading logic
    if (prices[i].Price <= lowerBand && cryptoBalance == 0)
    {
        // Buy signal - price touched lower band
        decimal amountToSpend = cashBalance * 0.98m; // 98% of cash, leaving some for fees
        decimal size = amountToSpend / prices[i].Price;
        
        ExecuteTrade(OrderSide.Buy, prices[i].Timestamp, prices[i].Price, size, ""Price touched lower band"");
    }
    else if (prices[i].Price >= upperBand && cryptoBalance > 0)
    {
        // Sell signal - price touched upper band
        ExecuteTrade(OrderSide.Sell, prices[i].Timestamp, prices[i].Price, cryptoBalance, ""Price touched upper band"");
    }
}"
        };

        // RSI Strategy
        var rsiStrategy = new BacktestStrategy
        {
            Name = "RSI",
            Description = "Uses the Relative Strength Index to identify overbought and oversold conditions",
            Parameters = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                { "Period", "14" },
                { "OversoldThreshold", "30" },
                { "OverboughtThreshold", "70" }
            }),
            Code = @"
// RSI Strategy
// Buy when RSI falls below oversold threshold
// Sell when RSI rises above overbought threshold

var period = int.Parse(parameters[""Period""]);
var oversoldThreshold = int.Parse(parameters[""OversoldThreshold""]);
var overboughtThreshold = int.Parse(parameters[""OverboughtThreshold""]);

// Calculate price changes
var priceChanges = new List<decimal>();
for (int i = 1; i < prices.Count; i++)
{
    priceChanges.Add(prices[i].Price - prices[i - 1].Price);
}

// Calculate RSI
var rsiValues = new List<decimal>();
for (int i = 0; i < period; i++)
{
    rsiValues.Add(50); // Default value
}

for (int i = period; i < priceChanges.Count; i++)
{
    decimal avgGain = 0;
    decimal avgLoss = 0;
    
    for (int j = i - period; j < i; j++)
    {
        if (priceChanges[j] >= 0)
        {
            avgGain += priceChanges[j];
        }
        else
        {
            avgLoss += Math.Abs(priceChanges[j]);
        }
    }
    
    avgGain /= period;
    avgLoss /= period;
    
    decimal rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
    decimal rsi = 100 - (100 / (1 + rs));
    
    rsiValues.Add(rsi);
    
    // Trading logic (i+1 because rsiValues has an offset compared to prices)
    if (rsi <= oversoldThreshold && cryptoBalance == 0)
    {
        // Buy signal - RSI is oversold
        decimal amountToSpend = cashBalance * 0.98m; // 98% of cash, leaving some for fees
        decimal size = amountToSpend / prices[i + 1].Price;
        
        ExecuteTrade(OrderSide.Buy, prices[i + 1].Timestamp, prices[i + 1].Price, size, $""RSI oversold: {rsi}"");
    }
    else if (rsi >= overboughtThreshold && cryptoBalance > 0)
    {
        // Sell signal - RSI is overbought
        ExecuteTrade(OrderSide.Sell, prices[i + 1].Timestamp, prices[i + 1].Price, cryptoBalance, $""RSI overbought: {rsi}"");
    }
}"
        };

        _strategies[smaCrossover.Name] = smaCrossover;
        _strategies[bollingerBands.Name] = bollingerBands;
        _strategies[rsiStrategy.Name] = rsiStrategy;
    }

    public async Task<BacktestResult> RunBacktestAsync(
        string strategyName,
        string productId,
        DateTime startDate,
        DateTime endDate,
        decimal initialBalance,
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        // Validate strategy exists
        if (!_strategies.TryGetValue(strategyName, out var strategy))
        {
            throw new KeyNotFoundException($"Strategy '{strategyName}' not found");
        }

        // Validate product exists
        var product = await _productService.GetProductAsync(productId, cancellationToken);

        // Get historical price data
        var priceHistory = await _priceService.GetPriceHistoryAsync(
            productId,
            startDate,
            endDate,
            cancellationToken);

        var prices = priceHistory.ToList();

        if (!prices.Any())
        {
            throw new InvalidOperationException("No price data available for the specified period");
        }

        // Create a new backtest result
        var result = new BacktestResult
        {
            StrategyName = strategyName,
            StartDate = startDate,
            EndDate = endDate,
            ProductId = productId,
            InitialBalance = initialBalance
        };

        // Run the backtest (simplified implementation)
        await SimulateBacktestAsync(result, strategy, prices, initialBalance, parameters);

        // Store the result
        _results[result.Id] = result;

        return result;
    }

    private async Task SimulateBacktestAsync(
        BacktestResult result,
        BacktestStrategy strategy,
        List<PricePoint> prices,
        decimal initialBalance,
        IDictionary<string, string> parameters)
    {
        // Initialize variables for the backtest
        decimal cashBalance = initialBalance;
        decimal cryptoBalance = 0;

        // Track equity curve (portfolio value over time)
        foreach (var price in prices)
        {
            decimal portfolioValue = cashBalance + (cryptoBalance * price.Price);

            result.EquityCurve.Add(new BacktestDataPoint
            {
                Timestamp = price.Timestamp,
                Price = price.Price,
                PortfolioValue = portfolioValue
            });
        }

        // This is where we would execute the strategy code
        // For simplicity, let's just simulate some trades

        // Simple SMA crossover strategy example
        if (strategy.Name == "SMA Crossover")
        {
            int shortPeriod = int.Parse(parameters["ShortPeriod"]);
            int longPeriod = int.Parse(parameters["LongPeriod"]);

            // Calculate SMAs
            var shortSma = CalculateSMA(prices, shortPeriod);
            var longSma = CalculateSMA(prices, longPeriod);

            // Trading logic
            bool lastCrossed = false;
            for (int i = longPeriod; i < prices.Count; i++)
            {
                bool isCrossedAbove = shortSma[i] > longSma[i];

                if (isCrossedAbove && !lastCrossed && cryptoBalance == 0)
                {
                    // Buy signal - short SMA crossed above long SMA
                    decimal amountToSpend = cashBalance * 0.98m; // 98% of cash, leaving some for fees
                    decimal size = amountToSpend / prices[i].Price;

                    ExecuteTrade(result, OrderSide.Buy, prices[i].Timestamp, prices[i].Price, size, "SMA Crossed Above", ref cashBalance, ref cryptoBalance);
                }
                else if (!isCrossedAbove && lastCrossed && cryptoBalance > 0)
                {
                    // Sell signal - short SMA crossed below long SMA
                    ExecuteTrade(result, OrderSide.Sell, prices[i].Timestamp, prices[i].Price, cryptoBalance, "SMA Crossed Below", ref cashBalance, ref cryptoBalance);
                }

                lastCrossed = isCrossedAbove;
            }
        }

        // Calculate final results
        decimal finalPortfolioValue = cashBalance + (cryptoBalance * prices.Last().Price);
        result.FinalBalance = finalPortfolioValue;
        result.ProfitLoss = finalPortfolioValue - initialBalance;
        result.ProfitLossPercent = (result.ProfitLoss / initialBalance) * 100;
        result.TotalTrades = result.Trades.Count;
    }

    private List<decimal> CalculateSMA(List<PricePoint> prices, int period)
    {
        var result = new List<decimal>();

        // Initialize with zeros for the first period-1 elements
        for (int i = 0; i < period - 1; i++)
        {
            result.Add(0);
        }

        // Calculate SMA for the rest
        for (int i = period - 1; i < prices.Count; i++)
        {
            decimal sum = 0;
            for (int j = i - period + 1; j <= i; j++)
            {
                sum += prices[j].Price;
            }
            result.Add(sum / period);
        }

        return result;
    }

    private void ExecuteTrade(
        BacktestResult result,
        OrderSide side,
        DateTime timestamp,
        decimal price,
        decimal size,
        string reason,
        ref decimal cashBalance,
        ref decimal cryptoBalance)
    {
        decimal value = price * size;
        decimal fee = value * 0.005m; // 0.5% fee

        if (side == OrderSide.Buy)
        {
            // Deduct cash, add crypto
            cashBalance -= (value + fee);
            cryptoBalance += size;
        }
        else
        {
            // Add cash, deduct crypto
            cashBalance += (value - fee);
            cryptoBalance -= size;
        }

        // Record the trade
        result.Trades.Add(new BacktestTrade
        {
            Timestamp = timestamp,
            Side = side,
            Price = price,
            Size = size,
            Value = value,
            Reason = reason
        });

        // Update equity curve with the trade
        decimal portfolioValue = cashBalance + (cryptoBalance * price);

        // Find the equity curve point for this timestamp or add a new one
        var equityPoint = result.EquityCurve.FirstOrDefault(p => p.Timestamp == timestamp);
        if (equityPoint != null)
        {
            equityPoint.PortfolioValue = portfolioValue;
        }
        else
        {
            result.EquityCurve.Add(new BacktestDataPoint
            {
                Timestamp = timestamp,
                Price = price,
                PortfolioValue = portfolioValue
            });
        }
    }

    public Task<IEnumerable<BacktestStrategy>> GetAvailableStrategiesAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<BacktestStrategy>>(_strategies.Values.ToList());
    }

    public Task<BacktestStrategy> GetStrategyAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (!_strategies.TryGetValue(name, out var strategy))
        {
            throw new KeyNotFoundException($"Strategy '{name}' not found");
        }

        return Task.FromResult(strategy);
    }

    public Task<BacktestResult> GetBacktestResultAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!_results.TryGetValue(id, out var result))
        {
            throw new KeyNotFoundException($"Backtest result '{id}' not found");
        }

        return Task.FromResult(result);
    }

    public Task<IEnumerable<BacktestResult>> GetBacktestResultsAsync(
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var results = _results.Values
            .OrderByDescending(r => r.EndDate)
            .Take(limit)
            .ToList();

        return Task.FromResult<IEnumerable<BacktestResult>>(results);
    }

    public Task SaveStrategyAsync(
        BacktestStrategy strategy,
        CancellationToken cancellationToken = default)
    {
        _strategies[strategy.Name] = strategy;
        return Task.CompletedTask;
    }
}