// CoinbaseSandbox.Api/Controllers/BacktestController.cs
namespace CoinbaseSandbox.Api.Controllers;

using CoinbaseSandbox.Domain.Models;
using CoinbaseSandbox.Domain.Services;
using CoinbaseSandbox.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

/// <summary>
/// Controller for backtesting trading strategies
/// </summary>
[ApiController]
[Route("api/v3/sandbox/backtest")]
public class BacktestController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IPriceService _priceService;
    private readonly IWalletService _walletService;
    private readonly IProductRepository _productRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPriceRepository _priceRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<BacktestController> _logger;
    
    public BacktestController(
        IProductService productService,
        IOrderService orderService,
        IPriceService priceService,
        IWalletService walletService,
        IProductRepository productRepository,
        IWalletRepository walletRepository,
        IPriceRepository priceRepository,
        IOrderRepository orderRepository,
        ILogger<BacktestController> logger)
    {
        _productService = productService;
        _orderService = orderService;
        _priceService = priceService;
        _walletService = walletService;
        _productRepository = productRepository;
        _walletRepository = walletRepository;
        _priceRepository = priceRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }
    
    public class PriceDataPoint
    {
        public DateTime Timestamp { get; set; }
        public decimal Price { get; set; }
    }
    
    public class BacktestRequest
    {
        public string ProductId { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public decimal InitialFiatBalance { get; set; } = 10000m;
        public decimal InitialCryptoBalance { get; set; } = 0m;
        public string PriceDataSource { get; set; } = "generated"; // "generated", "file", "api"
        public string PriceModel { get; set; } = "random"; // "random", "trend", "historical"
        public decimal? TrendStartPrice { get; set; }
        public decimal? TrendEndPrice { get; set; }
        public decimal? VolatilityPercent { get; set; } = 2.0m;
        public List<PriceDataPoint>? CustomPriceData { get; set; }
        public StrategyParameters Strategy { get; set; } = new StrategyParameters();
        public bool CreateWallet { get; set; } = true;
        
        public class StrategyParameters
        {
            public string Type { get; set; } = "simple"; // "simple", "movingAverage", "custom"
            public decimal? BuyThreshold { get; set; }   // For simple strategy
            public decimal? SellThreshold { get; set; }  // For simple strategy
            public int? ShortPeriod { get; set; }        // For moving average strategy
            public int? LongPeriod { get; set; }         // For moving average strategy
            public decimal? StopLossPercent { get; set; }
            public decimal? TakeProfitPercent { get; set; }
            public string? CustomLogic { get; set; }     // For custom strategy (would contain strategy logic)
        }
    }
    
    /// <summary>
    /// Runs a backtest of a trading strategy using historical or generated price data
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RunBacktest([FromBody] BacktestRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.ProductId))
            {
                return BadRequest(new { error = "Product ID is required" });
            }
            
            // Parse dates
            if (!DateTime.TryParse(request.StartDate, out var startDate))
            {
                return BadRequest(new { error = "Invalid start date format" });
            }
            
            if (!DateTime.TryParse(request.EndDate, out var endDate))
            {
                return BadRequest(new { error = "Invalid end date format" });
            }
            
            if (startDate >= endDate)
            {
                return BadRequest(new { error = "Start date must be before end date" });
            }
            
            // Verify the product exists
            try
            {
                var product = await _productService.GetProductAsync(request.ProductId, cancellationToken);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Product {request.ProductId} not found" });
            }
            
            // Generate or retrieve price data
            var priceData = await GeneratePriceDataAsync(request, startDate, endDate, cancellationToken);
            
            if (priceData.Count < 2)
            {
                return BadRequest(new { error = "Insufficient price data for backtest" });
            }
            
            // Create a backtest wallet if requested
            string walletId = "backtest-" + Guid.NewGuid().ToString();
            
            if (request.CreateWallet)
            {
                // Parse the product ID to get currencies
                var parts = request.ProductId.Split('-');
                if (parts.Length != 2)
                {
                    return BadRequest(new { error = "Invalid product ID format" });
                }
                
                var baseCurrency = parts[0]; // e.g., BTC
                var quoteCurrency = parts[1]; // e.g., USD
                
                // Create the wallet with initial balances
                var wallet = new Wallet(walletId, $"Backtest Wallet ({request.ProductId})");
                
                wallet.AddAsset(new Asset(new Currency(baseCurrency, baseCurrency), request.InitialCryptoBalance));
                wallet.AddAsset(new Asset(new Currency(quoteCurrency, quoteCurrency), request.InitialFiatBalance));
                
                await _walletRepository.AddAsync(wallet, cancellationToken);
            }
            
            // Run the backtest
            var result = await RunBacktestStrategyAsync(request, walletId, priceData, cancellationToken);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running backtest");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Generates price data based on the request parameters
    /// </summary>
    private async Task<List<PriceDataPoint>> GeneratePriceDataAsync(
        BacktestRequest request, 
        DateTime startDate, 
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var priceData = new List<PriceDataPoint>();
        
        switch (request.PriceDataSource.ToLower())
        {
            case "generated":
                // Generate synthetic price data
                switch (request.PriceModel.ToLower())
                {
                    case "trend":
                        // Generate a price trend
                        if (!request.TrendStartPrice.HasValue || !request.TrendEndPrice.HasValue)
                        {
                            throw new ArgumentException("Start and end prices are required for trend model");
                        }
                        
                        priceData = GenerateTrendPriceData(
                            startDate, 
                            endDate, 
                            request.TrendStartPrice.Value, 
                            request.TrendEndPrice.Value,
                            request.VolatilityPercent);
                        break;
                        
                    case "random":
                    default:
                        // Generate random price data
                        decimal startPrice = 0;
                        
                        // Try to get the current price or use a default
                        try
                        {
                            startPrice = await _priceService.GetCurrentPriceAsync(request.ProductId, cancellationToken);
                        }
                        catch
                        {
                            // If we can't get the price, use a reasonable default based on the product
                            if (request.ProductId.StartsWith("BTC-"))
                                startPrice = 50000m;
                            else if (request.ProductId.StartsWith("ETH-"))
                                startPrice = 3000m;
                            else if (request.ProductId.StartsWith("SOL-"))
                                startPrice = 150m;
                            else
                                startPrice = 100m;
                        }
                        
                        priceData = GenerateRandomPriceData(
                            startDate, 
                            endDate, 
                            startPrice,
                            request.VolatilityPercent);
                        break;
                }
                break;
                
            case "file":
                // Use custom price data from the request
                if (request.CustomPriceData == null || !request.CustomPriceData.Any())
                {
                    throw new ArgumentException("Custom price data is required when using 'file' data source");
                }
                
                priceData = request.CustomPriceData
                    .Where(p => p.Timestamp >= startDate && p.Timestamp <= endDate)
                    .OrderBy(p => p.Timestamp)
                    .ToList();
                break;
                
            case "api":
                // For now, we'll treat this the same as "generated"
                // In a real implementation, we'd fetch historical data from an API
                decimal apiStartPrice = 0;
                
                // Try to get the current price or use a default
                try
                {
                    apiStartPrice = await _priceService.GetCurrentPriceAsync(request.ProductId, cancellationToken);
                }
                catch
                {
                    // If we can't get the price, use a reasonable default based on the product
                    if (request.ProductId.StartsWith("BTC-"))
                        apiStartPrice = 50000m;
                    else if (request.ProductId.StartsWith("ETH-"))
                        apiStartPrice = 3000m;
                    else if (request.ProductId.StartsWith("SOL-"))
                        apiStartPrice = 150m;
                    else
                        apiStartPrice = 100m;
                }
                
                priceData = GenerateRandomPriceData(
                    startDate, 
                    endDate, 
                    apiStartPrice,
                    request.VolatilityPercent);
                break;
        }
        
        return priceData;
    }
    
    /// <summary>
    /// Generates price data with a trend from start to end price
    /// </summary>
    private List<PriceDataPoint> GenerateTrendPriceData(
        DateTime startDate,
        DateTime endDate,
        decimal startPrice,
        decimal endPrice,
        decimal? volatilityPercent)
    {
        var priceData = new List<PriceDataPoint>();
        var random = new Random();
        
        // Calculate the number of data points (1 per hour)
        var totalHours = (int)(endDate - startDate).TotalHours;
        if (totalHours <= 0) totalHours = 24; // Minimum 24 data points
        
        // Calculate the price change per hour
        var totalPriceChange = endPrice - startPrice;
        var priceChangePerHour = totalPriceChange / totalHours;
        
        // Set volatility
        var volatility = volatilityPercent.HasValue ? volatilityPercent.Value : 2.0m;
        
        // Generate the data points
        for (int i = 0; i <= totalHours; i++)
        {
            var timestamp = startDate.AddHours(i);
            var trendPrice = startPrice + (priceChangePerHour * i);
            
            // Add random noise based on volatility
            var randomFactor = 1 + (((decimal)random.NextDouble() * 2 - 1) * volatility / 100);
            var price = trendPrice * randomFactor;
            
            // Ensure price is positive
            if (price <= 0) price = 0.01m;
            
            priceData.Add(new PriceDataPoint 
            { 
                Timestamp = timestamp, 
                Price = price 
            });
        }
        
        return priceData;
    }
    
    /// <summary>
    /// Generates random price data with the given volatility
    /// </summary>
    private List<PriceDataPoint> GenerateRandomPriceData(
        DateTime startDate,
        DateTime endDate,
        decimal startPrice,
        decimal? volatilityPercent)
    {
        var priceData = new List<PriceDataPoint>();
        var random = new Random();
        
        // Calculate the number of data points (1 per hour)
        var totalHours = (int)(endDate - startDate).TotalHours;
        if (totalHours <= 0) totalHours = 24; // Minimum 24 data points
        
        // Set volatility
        var volatility = volatilityPercent.HasValue ? volatilityPercent.Value : 2.0m;
        var maxDailyChange = volatility / 100m;
        
        // Generate the data points
        var currentPrice = startPrice;
        
        for (int i = 0; i <= totalHours; i++)
        {
            var timestamp = startDate.AddHours(i);
            
            // Random price movement based on volatility
            var change = ((decimal)random.NextDouble() * 2 - 1) * maxDailyChange;
            currentPrice = currentPrice * (1 + change);
            
            // Ensure price is positive
            if (currentPrice <= 0) currentPrice = 0.01m;
            
            priceData.Add(new PriceDataPoint 
            { 
                Timestamp = timestamp, 
                Price = currentPrice 
            });
        }
        
        return priceData;
    }
    
    public class BacktestResult
    {
        public string WalletId { get; set; } = string.Empty;
        public decimal InitialInvestment { get; set; }
        public decimal FinalPortfolioValue { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal ProfitLossPercent { get; set; }
        public int TotalTrades { get; set; }
        public int WinningTrades { get; set; }
        public int LosingTrades { get; set; }
        public decimal AverageWin { get; set; }
        public decimal AverageLoss { get; set; }
        public decimal MaxDrawdown { get; set; }
        public List<TradeResult> Trades { get; set; } = new List<TradeResult>();
        public Dictionary<string, decimal> FinalBalances { get; set; } = new Dictionary<string, decimal>();
        public List<PortfolioValuePoint> PortfolioHistory { get; set; } = new List<PortfolioValuePoint>();
        
        public class TradeResult
        {
            public DateTime Timestamp { get; set; }
            public string Side { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public decimal Size { get; set; }
            public decimal Value { get; set; }
            public decimal PortfolioValueBefore { get; set; }
            public decimal PortfolioValueAfter { get; set; }
            public string TriggerReason { get; set; } = string.Empty;
        }
        
        public class PortfolioValuePoint
        {
            public DateTime Timestamp { get; set; }
            public decimal Price { get; set; }
            public decimal PortfolioValue { get; set; }
            public Dictionary<string, decimal> Balances { get; set; } = new Dictionary<string, decimal>();
        }
    }
    
    /// <summary>
    /// Runs the backtest strategy against the price data
    /// </summary>
    private async Task<BacktestResult> RunBacktestStrategyAsync(
        BacktestRequest request,
        string walletId,
        List<PriceDataPoint> priceData,
        CancellationToken cancellationToken)
    {
        var result = new BacktestResult { WalletId = walletId };
        
        // Parse the product ID to get currencies
        var parts = request.ProductId.Split('-');
        if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid product ID format");
        }
        
        var baseCurrency = parts[0]; // e.g., BTC
        var quoteCurrency = parts[1]; // e.g., USD
        
        // Get wallet and initial balances
        Wallet wallet;
        try
        {
            wallet = await _walletService.GetWalletAsync(walletId, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            // Create a new wallet if not found
            wallet = new Wallet(walletId, $"Backtest Wallet ({request.ProductId})");
            wallet.AddAsset(new Asset(new Currency(baseCurrency, baseCurrency), request.InitialCryptoBalance));
            wallet.AddAsset(new Asset(new Currency(quoteCurrency, quoteCurrency), request.InitialFiatBalance));
            await _walletRepository.AddAsync(wallet, cancellationToken);
        }
        
        // Calculate initial investment value
        decimal initialBaseBalance = 0;
        decimal initialQuoteBalance = 0;
        
        try
        {
            initialBaseBalance = wallet.GetAsset(baseCurrency).Balance;
        }
        catch { /* Ignore if not found */ }
        
        try
        {
            initialQuoteBalance = wallet.GetAsset(quoteCurrency).Balance;
        }
        catch { /* Ignore if not found */ }
        
        // Calculate initial investment in quote currency
        decimal initialInvestment = initialQuoteBalance + (initialBaseBalance * priceData.First().Price);
        result.InitialInvestment = initialInvestment;
        
        // Initialize strategy variables
        Dictionary<string, decimal> balances = new Dictionary<string, decimal>
        {
            { baseCurrency, initialBaseBalance },
            { quoteCurrency, initialQuoteBalance }
        };
        
        // For moving average strategy
        var shortPeriod = request.Strategy.ShortPeriod ?? 5; // Default to 5 hours
        var longPeriod = request.Strategy.LongPeriod ?? 20;  // Default to 20 hours
        var shortMa = new Queue<decimal>(shortPeriod);
        var longMa = new Queue<decimal>(longPeriod);
        decimal lastShortMa = 0;
        decimal lastLongMa = 0;
        bool wasShortAboveLong = false; // Used for moving average crossover
        
        // For simple strategy
        var buyThreshold = request.Strategy.BuyThreshold ?? -0.02m; // Default to buy when price drops 2%
        var sellThreshold = request.Strategy.SellThreshold ?? 0.03m; // Default to sell when price rises 3%
        decimal lastTradePrice = priceData.First().Price;
        
        // For stop loss / take profit
        decimal? entryPrice = null;
        if (initialBaseBalance > 0)
        {
            // If we start with crypto, set the entry price to the initial price
            entryPrice = priceData.First().Price;
        }
        
        var stopLossPercent = request.Strategy.StopLossPercent ?? 0.05m; // Default 5% stop loss
        var takeProfitPercent = request.Strategy.TakeProfitPercent ?? 0.10m; // Default 10% take profit
        
        // For tracking portfolio value over time
        var portfolioHistory = new List<BacktestResult.PortfolioValuePoint>();
        decimal maxPortfolioValue = initialInvestment;
        decimal minPortfolioValue = initialInvestment;
        
        // Process each price point
        foreach (var dataPoint in priceData)
        {
            // Calculate current portfolio value
            decimal baseBalance = balances[baseCurrency];
            decimal quoteBalance = balances[quoteCurrency];
            decimal portfolioValue = quoteBalance + (baseBalance * dataPoint.Price);
            
            // Track maximum portfolio value for drawdown calculation
            if (portfolioValue > maxPortfolioValue)
            {
                maxPortfolioValue = portfolioValue;
            }
            
            // Track portfolio history
            portfolioHistory.Add(new BacktestResult.PortfolioValuePoint
            {
                Timestamp = dataPoint.Timestamp,
                Price = dataPoint.Price,
                PortfolioValue = portfolioValue,
                Balances = new Dictionary<string, decimal>(balances)
            });
            
            // Update moving averages
            shortMa.Enqueue(dataPoint.Price);
            longMa.Enqueue(dataPoint.Price);
            
            if (shortMa.Count > shortPeriod)
                shortMa.Dequeue();
                
            if (longMa.Count > longPeriod)
                longMa.Dequeue();
                
            decimal currentShortMa = shortMa.Any() ? shortMa.Average() : dataPoint.Price;
            decimal currentLongMa = longMa.Any() ? longMa.Average() : dataPoint.Price;
            
            // Determine if we should make a trade based on the strategy
            bool shouldBuy = false;
            bool shouldSell = false;
            string tradeReason = string.Empty;
            
            switch (request.Strategy.Type.ToLower())
            {
                case "movingaverage":
                    // Moving average crossover strategy
                    bool isShortAboveLong = currentShortMa > currentLongMa;
                    
                    // Buy when short MA crosses above long MA
                    if (isShortAboveLong && !wasShortAboveLong && baseBalance == 0)
                    {
                        shouldBuy = true;
                        tradeReason = "Moving average crossover (short above long)";
                    }
                    
                    // Sell when short MA crosses below long MA
                    else if (!isShortAboveLong && wasShortAboveLong && baseBalance > 0)
                    {
                        shouldSell = true;
                        tradeReason = "Moving average crossover (short below long)";
                    }
                    
                    wasShortAboveLong = isShortAboveLong;
                    break;
                    
                case "simple":
                default:
                    // Simple price threshold strategy
                    decimal priceChange = (dataPoint.Price - lastTradePrice) / lastTradePrice;
                    
                    // Buy when price drops by the threshold
                    if (priceChange <= buyThreshold && baseBalance == 0)
                    {
                        shouldBuy = true;
                        tradeReason = $"Price dropped by {buyThreshold * 100:0.##}%";
                    }
                    
                    // Sell when price rises by the threshold
                    else if (priceChange >= sellThreshold && baseBalance > 0)
                    {
                        shouldSell = true;
                        tradeReason = $"Price rose by {sellThreshold * 100:0.##}%";
                    }
                    break;
            }
            
            // Check stop loss / take profit if we have crypto
            if (baseBalance > 0 && entryPrice.HasValue)
            {
                decimal currentReturn = (dataPoint.Price - entryPrice.Value) / entryPrice.Value;
                
                // Stop loss
                if (currentReturn <= -stopLossPercent)
                {
                    shouldSell = true;
                    tradeReason = $"Stop loss triggered at {stopLossPercent * 100:0.##}%";
                }
                
                // Take profit
                else if (currentReturn >= takeProfitPercent)
                {
                    shouldSell = true;
                    tradeReason = $"Take profit triggered at {takeProfitPercent * 100:0.##}%";
                }
            }
            
            // Execute trades
            if (shouldBuy && quoteBalance > 0)
            {
                // Buy using all available quote currency
                decimal amountToSpend = quoteBalance * 0.99m; // Leave small buffer for fees
                decimal size = amountToSpend / dataPoint.Price;
                decimal fee = amountToSpend * 0.005m; // 0.5% fee
                
                // Update balances
                balances[baseCurrency] += size;
                balances[quoteCurrency] -= (amountToSpend + fee);
                
                // Set entry price for stop loss / take profit
                entryPrice = dataPoint.Price;
                
                // Record the trade
                result.Trades.Add(new BacktestResult.TradeResult
                {
                    Timestamp = dataPoint.Timestamp,
                    Side = "buy",
                    Price = dataPoint.Price,
                    Size = size,
                    Value = amountToSpend,
                    PortfolioValueBefore = portfolioValue,
                    PortfolioValueAfter = balances[quoteCurrency] + (balances[baseCurrency] * dataPoint.Price),
                    TriggerReason = tradeReason
                });
                
                // Update last trade price
                lastTradePrice = dataPoint.Price;
            }
            else if (shouldSell && baseBalance > 0)
            {
                // Sell all base currency
                decimal size = baseBalance;
                decimal value = size * dataPoint.Price;
                decimal fee = value * 0.005m; // 0.5% fee
                
                // Update balances
                balances[baseCurrency] = 0;
                balances[quoteCurrency] += (value - fee);
                
                // Reset entry price
                entryPrice = null;
                
                // Record the trade
                result.Trades.Add(new BacktestResult.TradeResult
                {
                    Timestamp = dataPoint.Timestamp,
                    Side = "sell",
                    Price = dataPoint.Price,
                    Size = size,
                    Value = value,
                    PortfolioValueBefore = portfolioValue,
                    PortfolioValueAfter = balances[quoteCurrency] + (balances[baseCurrency] * dataPoint.Price),
                    TriggerReason = tradeReason
                });
                
                // Update last trade price
                lastTradePrice = dataPoint.Price;
            }
            
            // Update last moving average values
            lastShortMa = currentShortMa;
            lastLongMa = currentLongMa;
        }
        
        // Calculate final portfolio value
        decimal finalBaseBalance = balances[baseCurrency];
        decimal finalQuoteBalance = balances[quoteCurrency];
        decimal finalPrice = priceData.Last().Price;
        decimal finalPortfolioValue = finalQuoteBalance + (finalBaseBalance * finalPrice);
        
        // Calculate profit/loss
        decimal profitLoss = finalPortfolioValue - initialInvestment;
        decimal profitLossPercent = initialInvestment > 0 ? (profitLoss / initialInvestment) * 100 : 0;
        
        // Calculate trade statistics
        int totalTrades = result.Trades.Count;
        var winningTrades = result.Trades.Where(t => t.PortfolioValueAfter > t.PortfolioValueBefore).ToList();
        var losingTrades = result.Trades.Where(t => t.PortfolioValueAfter < t.PortfolioValueBefore).ToList();
        
        decimal averageWin = winningTrades.Any() 
            ? winningTrades.Average(t => (t.PortfolioValueAfter - t.PortfolioValueBefore) / t.PortfolioValueBefore) * 100 
            : 0;
            
        decimal averageLoss = losingTrades.Any() 
            ? losingTrades.Average(t => (t.PortfolioValueBefore - t.PortfolioValueAfter) / t.PortfolioValueBefore) * 100 
            : 0;
            
        // Calculate maximum drawdown
        decimal maxDrawdown = 0;
        if (portfolioHistory.Count > 0)
        {
            decimal runningMax = portfolioHistory[0].PortfolioValue;
            
            foreach (var point in portfolioHistory)
            {
                if (point.PortfolioValue > runningMax)
                {
                    runningMax = point.PortfolioValue;
                }
                else
                {
                    decimal drawdown = (runningMax - point.PortfolioValue) / runningMax * 100;
                    if (drawdown > maxDrawdown)
                    {
                        maxDrawdown = drawdown;
                    }
                }
            }
        }
        
        // Populate the result
        result.FinalPortfolioValue = finalPortfolioValue;
        result.ProfitLoss = profitLoss;
        result.ProfitLossPercent = profitLossPercent;
        result.TotalTrades = totalTrades;
        result.WinningTrades = winningTrades.Count;
        result.LosingTrades = losingTrades.Count;
        result.AverageWin = averageWin;
        result.AverageLoss = averageLoss;
        result.MaxDrawdown = maxDrawdown;
        result.FinalBalances = balances;
        result.PortfolioHistory = portfolioHistory;
        
        return result;
    }
    
    /// <summary>
    /// Retrieves a previously run backtest result
    /// </summary>
    [HttpGet("{walletId}")]
    public async Task<IActionResult> GetBacktestResult(string walletId, CancellationToken cancellationToken)
    {
        try
        {
            // Get the backtest wallet
            var wallet = await _walletService.GetWalletAsync(walletId, cancellationToken);
            
            // In a real implementation, we would retrieve the full backtest result
            // For now, we'll just return the wallet balances
            var balances = wallet.Assets.ToDictionary(
                a => a.Currency.Symbol,
                a => a.Balance);
                
            return Ok(new
            {
                wallet_id = walletId,
                balances
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Backtest wallet {walletId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving backtest result");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}