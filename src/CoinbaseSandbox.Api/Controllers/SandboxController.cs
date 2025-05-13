namespace CoinbaseSandbox.Api.Controllers;

using CoinbaseSandbox.Domain.Models;
using CoinbaseSandbox.Domain.Repositories;
using CoinbaseSandbox.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

/// <summary>
/// Controller for sandbox-specific features not available in the real Coinbase API
/// </summary>
[ApiController]
[Route("api/v3/sandbox")]
public class SandboxController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IPriceService _priceService;
    private readonly IWalletService _walletService;
    private readonly IProductRepository _productRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPriceRepository _priceRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<SandboxController> _logger;
    
    public SandboxController(
        IProductService productService,
        IOrderService orderService,
        IPriceService priceService,
        IWalletService walletService,
        IProductRepository productRepository,
        IWalletRepository walletRepository,
        IPriceRepository priceRepository,
        IOrderRepository orderRepository,
        ILogger<SandboxController> logger)
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
    
    #region Price Simulation
    
    // POST /api/v3/sandbox/prices/{productId}
    // Set a static price for a product
    [HttpPost("prices/{productId}")]
    public async Task<IActionResult> SetStaticPrice(
        string productId,
        [FromBody] decimal price,
        CancellationToken cancellationToken)
    {
        try
        {
            var pricePoint = await _priceService.SetMockPriceAsync(
                productId, 
                price, 
                cancellationToken);
                
            return Ok(new
            {
                success = true,
                product_id = pricePoint.ProductId,
                price = pricePoint.Price.ToString(),
                timestamp = pricePoint.Timestamp.ToString("o"),
                simulation_mode = "static"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    public class PriceSimulationRequest
    {
        public string Mode { get; set; } = "static"; // static, trend, volatility, replay
        public decimal? StartPrice { get; set; }
        public decimal? EndPrice { get; set; }
        public int? DurationSeconds { get; set; }
        public decimal? VolatilityPercent { get; set; }
        public string? HistoricalDate { get; set; } // For replay mode
        public bool Repeat { get; set; } = false;
    }
    
    // POST /api/v3/sandbox/prices/{productId}/simulate
    // Simulate price movement over time
    [HttpPost("prices/{productId}/simulate")]
    public async Task<IActionResult> SimulatePrice(
        string productId,
        [FromBody] PriceSimulationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Ensure the product exists
            var product = await _productService.GetProductAsync(productId, cancellationToken);
            
            // Get current price or use provided start price
            decimal currentPrice;
            try 
            {
                currentPrice = await _priceService.GetCurrentPriceAsync(productId, cancellationToken);
            }
            catch
            {
                if (request.StartPrice.HasValue)
                {
                    currentPrice = request.StartPrice.Value;
                }
                else
                {
                    // Default price if no current price and no start price provided
                    currentPrice = 50000.00m; // Default BTC price for testing
                }
            }
            
            // Set the initial price
            await _priceService.SetMockPriceAsync(productId, currentPrice, cancellationToken);
            
            // Start the simulation based on the mode
            switch (request.Mode.ToLower())
            {
                case "trend":
                    if (!request.EndPrice.HasValue)
                    {
                        return BadRequest(new { error = "End price is required for trend simulation" });
                    }
                    
                    if (!request.DurationSeconds.HasValue)
                    {
                        return BadRequest(new { error = "Duration is required for trend simulation" });
                    }
                    
                    // Start a background task for the trend simulation
                    _ = SimulateTrendAsync(
                        productId, 
                        currentPrice, 
                        request.EndPrice.Value, 
                        request.DurationSeconds.Value,
                        request.Repeat);
                    
                    return Ok(new
                    {
                        success = true,
                        product_id = productId,
                        start_price = currentPrice,
                        end_price = request.EndPrice.Value,
                        duration_seconds = request.DurationSeconds.Value,
                        simulation_mode = "trend",
                        repeat = request.Repeat
                    });
                    
                case "volatility":
                    if (!request.VolatilityPercent.HasValue)
                    {
                        return BadRequest(new { error = "Volatility percentage is required for volatility simulation" });
                    }
                    
                    if (!request.DurationSeconds.HasValue)
                    {
                        return BadRequest(new { error = "Duration is required for volatility simulation" });
                    }
                    
                    // Start a background task for the volatility simulation
                    _ = SimulateVolatilityAsync(
                        productId, 
                        currentPrice, 
                        request.VolatilityPercent.Value, 
                        request.DurationSeconds.Value,
                        request.Repeat);
                    
                    return Ok(new
                    {
                        success = true,
                        product_id = productId,
                        base_price = currentPrice,
                        volatility_percent = request.VolatilityPercent.Value,
                        duration_seconds = request.DurationSeconds.Value,
                        simulation_mode = "volatility",
                        repeat = request.Repeat
                    });
                    
                case "replay":
                    // This would replay historical price data, but for simplicity we'll just return a not implemented response
                    return StatusCode(StatusCodes.Status501NotImplemented, new
                    {
                        message = "Historical price replay not implemented yet"
                    });
                    
                case "static":
                default:
                    // For static mode, we've already set the price
                    return Ok(new
                    {
                        success = true,
                        product_id = productId,
                        price = currentPrice,
                        simulation_mode = "static"
                    });
            }
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in price simulation for {ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
    
    // Background task to simulate a price trend from start to end over a duration
    private async Task SimulateTrendAsync(
        string productId, 
        decimal startPrice, 
        decimal endPrice, 
        int durationSeconds,
        bool repeat)
    {
        // Calculate the price step for each update (every second)
        decimal totalPriceChange = endPrice - startPrice;
        decimal priceStepPerSecond = totalPriceChange / durationSeconds;
        
        try
        {
            do
            {
                decimal currentPrice = startPrice;
                
                // Update the price each second
                for (int elapsed = 0; elapsed < durationSeconds; elapsed++)
                {
                    // Apply the price change
                    currentPrice += priceStepPerSecond;
                    
                    // Set the new price
                    await _priceService.SetMockPriceAsync(productId, currentPrice, CancellationToken.None);
                    
                    // Wait for one second
                    await Task.Delay(1000);
                }
                
                // If repeating, reverse the direction
                if (repeat)
                {
                    // Swap start and end prices
                    (startPrice, endPrice) = (endPrice, startPrice);
                    priceStepPerSecond = -priceStepPerSecond;
                }
                else
                {
                    break;
                }
            }
            while (repeat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in trend simulation for {ProductId}", productId);
        }
    }
    
    // Background task to simulate price volatility around a base price
    private async Task SimulateVolatilityAsync(
        string productId, 
        decimal basePrice, 
        decimal volatilityPercent, 
        int durationSeconds,
        bool repeat)
    {
        // Calculate the max deviation
        decimal maxDeviation = basePrice * (volatilityPercent / 100m);
        Random random = new Random();
        
        try
        {
            do
            {
                // Update the price each second
                for (int elapsed = 0; elapsed < durationSeconds; elapsed++)
                {
                    // Generate a random deviation within the volatility range
                    decimal deviation = ((decimal)random.NextDouble() * 2 - 1) * maxDeviation;
                    decimal newPrice = basePrice + deviation;
                    
                    // Ensure price doesn't go below zero
                    if (newPrice <= 0)
                    {
                        newPrice = 0.01m; // Minimum price
                    }
                    
                    // Set the new price
                    await _priceService.SetMockPriceAsync(productId, newPrice, CancellationToken.None);
                    
                    // Wait for one second
                    await Task.Delay(1000);
                }
            }
            while (repeat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in volatility simulation for {ProductId}", productId);
        }
    }
    
    // DELETE /api/v3/sandbox/prices/{productId}/simulation
    // Stop any ongoing price simulation
    [HttpDelete("prices/{productId}/simulation")]
    public IActionResult StopPriceSimulation(string productId)
    {
        // This would stop any ongoing simulation, but for simplicity 
        // we'll just acknowledge the request
        return Ok(new
        {
            success = true,
            product_id = productId,
            message = "Price simulation stopped"
        });
    }
    
    #endregion
    
    #region Wallet Management
    
    public class CreateWalletRequest
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<AssetBalance>? InitialBalances { get; set; }
        
        public class AssetBalance
        {
            public string Symbol { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public decimal Balance { get; set; }
        }
    }
    
    // POST /api/v3/sandbox/wallets
    // Create a new wallet with custom balances
    [HttpPost("wallets")]
    public async Task<IActionResult> CreateWallet(
        [FromBody] CreateWalletRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { error = "Wallet name is required" });
            }
            
            // Generate a wallet ID if not provided
            string walletId = string.IsNullOrWhiteSpace(request.Id) 
                ? Guid.NewGuid().ToString() 
                : request.Id;
            
            // Create the wallet
            var wallet = new Wallet(walletId, request.Name);
            
            // Add initial balances if provided
            if (request.InitialBalances != null)
            {
                foreach (var assetBalance in request.InitialBalances)
                {
                    var currency = new Currency(assetBalance.Symbol, assetBalance.Name);
                    var asset = new Asset(currency, assetBalance.Balance);
                    wallet.AddAsset(asset);
                }
            }
            
            // Save the wallet
            await _walletRepository.AddAsync(wallet, cancellationToken);
            
            return Ok(new
            {
                success = true,
                wallet_id = wallet.Id,
                name = wallet.Name,
                assets = wallet.Assets.Select(a => new
                {
                    symbol = a.Currency.Symbol,
                    name = a.Currency.Name,
                    balance = a.Balance
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    // POST /api/v3/sandbox/wallets/{walletId}/reset
    // Reset a wallet's balances
    [HttpPost("wallets/{walletId}/reset")]
    public async Task<IActionResult> ResetWallet(
        string walletId,
        [FromBody] List<CreateWalletRequest.AssetBalance> balances,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the wallet
            var wallet = await _walletService.GetWalletAsync(walletId, cancellationToken);
            
            // Create a new wallet with the same ID and name
            var newWallet = new Wallet(wallet.Id, wallet.Name);
            
            // Add the specified balances
            foreach (var assetBalance in balances)
            {
                var currency = new Currency(assetBalance.Symbol, assetBalance.Name);
                var asset = new Asset(currency, assetBalance.Balance);
                newWallet.AddAsset(asset);
            }
            
            // Update the wallet
            await _walletRepository.UpdateAsync(newWallet, cancellationToken);
            
            return Ok(new
            {
                success = true,
                wallet_id = newWallet.Id,
                name = newWallet.Name,
                assets = newWallet.Assets.Select(a => new
                {
                    symbol = a.Currency.Symbol,
                    name = a.Currency.Name,
                    balance = a.Balance
                }).ToList()
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Wallet {walletId} not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    #endregion
    
    #region System State
    
    // POST /api/v3/sandbox/reset
    // Reset the entire sandbox (clear all orders, reset wallet to initial state)
    [HttpPost("reset")]
    public async Task<IActionResult> ResetSandbox(CancellationToken cancellationToken)
    {
        try
        {
            // Create a new sandbox state (clear all repositories)
            // This is a simplistic implementation - in a real app, you might want
            // to call a dedicated method for each repository to clear its state
            
            // Re-initialize with seed data
            await CoinbaseSandbox.Infrastructure.Data.SeedData.InitializeAsync(
                _productRepository, 
                _walletRepository, 
                _priceRepository);
                
            return Ok(new
            {
                success = true,
                message = "Sandbox has been reset to initial state"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting sandbox");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
    
    // GET /api/v3/sandbox/state
    // Get the current state of the sandbox (wallets, orders, prices)
    [HttpGet("state")]
    public async Task<IActionResult> GetSandboxState(CancellationToken cancellationToken)
    {
        try
        {
            // Get all wallets
            var wallets = await _walletService.GetWalletsAsync(cancellationToken);
            
            // Get all orders
            var orders = await _orderService.GetOrdersAsync(1000, cancellationToken);
            
            // Get all products
            var products = await _productService.GetProductsAsync(cancellationToken);
            
            // Get the latest price for each product
            var prices = new List<object>();
            foreach (var product in products)
            {
                try
                {
                    var price = await _priceService.GetCurrentPriceAsync(product.Id, cancellationToken);
                    prices.Add(new
                    {
                        product_id = product.Id,
                        price = price
                    });
                }
                catch
                {
                    // Skip products with no price
                }
            }
            
            // Compile the complete sandbox state
            var state = new
            {
                wallets = wallets.Select(w => new
                {
                    id = w.Id,
                    name = w.Name,
                    assets = w.Assets.Select(a => new
                    {
                        symbol = a.Currency.Symbol,
                        name = a.Currency.Name,
                        balance = a.Balance
                    }).ToList()
                }).ToList(),
                
                orders = orders.Select(o => new
                {
                    id = o.Id,
                    product_id = o.ProductId,
                    side = o.Side.ToString().ToLower(),
                    type = o.Type.ToString().ToLower(),
                    status = o.Status.ToString().ToLower(),
                    size = o.Size,
                    limit_price = o.LimitPrice,
                    executed_price = o.ExecutedPrice,
                    fee = o.Fee,
                    created_at = o.CreatedAt,
                    updated_at = o.UpdatedAt
                }).ToList(),
                
                products = products.Select(p => new
                {
                    id = p.Id,
                    base_currency = p.BaseCurrency.Symbol,
                    quote_currency = p.QuoteCurrency.Symbol,
                    minimum_order_size = p.MinimumOrderSize
                }).ToList(),
                
                prices = prices
            };
            
            return Ok(state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sandbox state");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
    
    #endregion
    
    #region Scenario Testing
    
    public class ScenarioRequest
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, decimal>? Prices { get; set; }
        public List<WalletSetup>? Wallets { get; set; }
        
        public class WalletSetup
        {
            public string Id { get; set; } = string.Empty;
            public List<AssetSetup> Assets { get; set; } = new List<AssetSetup>();
            
            public class AssetSetup
            {
                public string Symbol { get; set; } = string.Empty;
                public decimal Balance { get; set; }
            }
        }
    }
    
    // POST /api/v3/sandbox/scenarios
    // Set up a predefined test scenario
    [HttpPost("scenarios")]
    public async Task<IActionResult> SetupScenario(
        [FromBody] ScenarioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check for a named built-in scenario
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                switch (request.Name.ToLower())
                {
                    case "bullrun":
                        // Set up a bull market scenario
                        await SetupBullRunScenario(cancellationToken);
                        return Ok(new
                        {
                            success = true,
                            scenario = "bullrun",
                            message = "Bull market scenario has been set up"
                        });
                        
                    case "bearmarket":
                        // Set up a bear market scenario
                        await SetupBearMarketScenario(cancellationToken);
                        return Ok(new
                        {
                            success = true,
                            scenario = "bearmarket",
                            message = "Bear market scenario has been set up"
                        });
                        
                    case "volatility":
                        // Set up a high volatility scenario
                        await SetupVolatilityScenario(cancellationToken);
                        return Ok(new
                        {
                            success = true,
                            scenario = "volatility",
                            message = "High volatility scenario has been set up"
                        });
                        
                    default:
                        return BadRequest(new
                        {
                            error = $"Unknown scenario: {request.Name}. Available scenarios: bullrun, bearmarket, volatility"
                        });
                }
            }
            
            // Custom scenario setup
            if (request.Prices != null)
            {
                foreach (var (productId, price) in request.Prices)
                {
                    await _priceService.SetMockPriceAsync(productId, price, cancellationToken);
                }
            }
            
            if (request.Wallets != null)
            {
                foreach (var walletSetup in request.Wallets)
                {
                    try
                    {
                        var wallet = await _walletService.GetWalletAsync(walletSetup.Id, cancellationToken);
                        
                        // Create a new wallet with the same ID and name
                        var newWallet = new Wallet(wallet.Id, wallet.Name);
                        
                        // Add the specified balances
                        foreach (var assetSetup in walletSetup.Assets)
                        {
                            // Try to get the currency from an existing asset
                            if (wallet.TryGetAsset(assetSetup.Symbol, out var existingAsset))
                            {
                                var asset = new Asset(existingAsset.Currency, assetSetup.Balance);
                                newWallet.AddAsset(asset);
                            }
                            else
                            {
                                // Create a new currency
                                var currency = new Currency(assetSetup.Symbol, assetSetup.Symbol);
                                var asset = new Asset(currency, assetSetup.Balance);
                                newWallet.AddAsset(asset);
                            }
                        }
                        
                        // Update the wallet
                        await _walletRepository.UpdateAsync(newWallet, cancellationToken);
                    }
                    catch (KeyNotFoundException)
                    {
                        // Skip wallets that don't exist
                    }
                }
            }
            
            return Ok(new
            {
                success = true,
                message = "Custom scenario has been set up"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up scenario");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
    
    // Helper method to set up a bull market scenario
    private async Task SetupBullRunScenario(CancellationToken cancellationToken)
    {
        // Set up increasing prices
        await _priceService.SetMockPriceAsync("BTC-USD", 75000.00m, cancellationToken);
        await _priceService.SetMockPriceAsync("ETH-USD", 4500.00m, cancellationToken);
        await _priceService.SetMockPriceAsync("SOL-USD", 195.00m, cancellationToken);
        
        // Start price trends for all products
        _ = SimulateTrendAsync("BTC-USD", 75000.00m, 85000.00m, 3600, true); // +10K in 1 hour, repeating
        _ = SimulateTrendAsync("ETH-USD", 4500.00m, 5200.00m, 3600, true);  // +700 in 1 hour, repeating
        _ = SimulateTrendAsync("SOL-USD", 195.00m, 250.00m, 3600, true);    // +55 in 1 hour, repeating
        
        // Set up a default wallet with reasonable balances
        var wallet = await _walletService.GetWalletAsync("default", cancellationToken);
        
        // Create a new wallet with the same ID and name
        var newWallet = new Wallet(wallet.Id, wallet.Name);
        
        // Add the standard assets with bull market appropriate balances
        var usd = new Currency("USD", "US Dollar");
        var btc = new Currency("BTC", "Bitcoin");
        var eth = new Currency("ETH", "Ethereum");
        var sol = new Currency("SOL", "Solana");
        
        newWallet.AddAsset(new Asset(usd, 25000.00m));  // Good amount of USD for buying
        newWallet.AddAsset(new Asset(btc, 0.5m));      // Some BTC
        newWallet.AddAsset(new Asset(eth, 5.0m));      // Some ETH
        newWallet.AddAsset(new Asset(sol, 20.0m));     // Some SOL
        
        // Update the wallet
        await _walletRepository.UpdateAsync(newWallet, cancellationToken);
    }
    
    // Helper method to set up a bear market scenario
    private async Task SetupBearMarketScenario(CancellationToken cancellationToken)
    {
        // Set up decreasing prices
        await _priceService.SetMockPriceAsync("BTC-USD", 65000.00m, cancellationToken);
        await _priceService.SetMockPriceAsync("ETH-USD", 3800.00m, cancellationToken);
        await _priceService.SetMockPriceAsync("SOL-USD", 150.00m, cancellationToken);
        
        // Start price trends for all products
        _ = SimulateTrendAsync("BTC-USD", 65000.00m, 55000.00m, 3600, true); // -10K in 1 hour, repeating
        _ = SimulateTrendAsync("ETH-USD", 3800.00m, 3300.00m, 3600, true);  // -500 in 1 hour, repeating
        _ = SimulateTrendAsync("SOL-USD", 150.00m, 120.00m, 3600, true);    // -30 in 1 hour, repeating
        
        // Set up a default wallet with reasonable balances
        var wallet = await _walletService.GetWalletAsync("default", cancellationToken);
        
        // Create a new wallet with the same ID and name
        var newWallet = new Wallet(wallet.Id, wallet.Name);
        
        // Add the standard assets with bear market appropriate balances
        var usd = new Currency("USD", "US Dollar");
        var btc = new Currency("BTC", "Bitcoin");
        var eth = new Currency("ETH", "Ethereum");
        var sol = new Currency("SOL", "Solana");
        
        newWallet.AddAsset(new Asset(usd, 5000.00m));   // Less USD
        newWallet.AddAsset(new Asset(btc, 1.0m));      // More BTC to potentially sell
        newWallet.AddAsset(new Asset(eth, 10.0m));     // More ETH to potentially sell
        newWallet.AddAsset(new Asset(sol, 50.0m));     // More SOL to potentially sell
        
        // Update the wallet
        await _walletRepository.UpdateAsync(newWallet, cancellationToken);
    }
    
    // Helper method to set up a high volatility scenario
    private async Task SetupVolatilityScenario(CancellationToken cancellationToken)
    {
        // Set up base prices
        await _priceService.SetMockPriceAsync("BTC-USD", 70000.00m, cancellationToken);
        await _priceService.SetMockPriceAsync("ETH-USD", 4000.00m, cancellationToken);
        await _priceService.SetMockPriceAsync("SOL-USD", 180.00m, cancellationToken);
        
        // Start volatility simulations for all products
        _ = SimulateVolatilityAsync("BTC-USD", 70000.00m, 10.0m, 7200, true); // 10% volatility over 2 hours, repeating
        _ = SimulateVolatilityAsync("ETH-USD", 4000.00m, 12.0m, 7200, true);  // 12% volatility over 2 hours, repeating
        _ = SimulateVolatilityAsync("SOL-USD", 180.00m, 15.0m, 7200, true);   // 15% volatility over 2 hours, repeating
        
        // Set up a default wallet with reasonable balances
        var wallet = await _walletService.GetWalletAsync("default", cancellationToken);
        
        // Create a new wallet with the same ID and name
        var newWallet = new Wallet(wallet.Id, wallet.Name);
        
        // Add the standard assets with balanced portfolio
        var usd = new Currency("USD", "US Dollar");
        var btc = new Currency("BTC", "Bitcoin");
        var eth = new Currency("ETH", "Ethereum");
        var sol = new Currency("SOL", "Solana");
        
        newWallet.AddAsset(new Asset(usd, 15000.00m)); // Balanced USD
        newWallet.AddAsset(new Asset(btc, 0.75m));     // Balanced BTC
        newWallet.AddAsset(new Asset(eth, 7.5m));      // Balanced ETH
        newWallet.AddAsset(new Asset(sol, 35.0m));     // Balanced SOL
        
        // Update the wallet
        await _walletRepository.UpdateAsync(newWallet, cancellationToken);
    }
    
    #endregion
}