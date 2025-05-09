namespace CoinbaseSandbox.Infrastructure.Data;

using Domain.Models;
using CoinbaseSandbox.Domain.Repositories;

public static class SeedData
{
    public static async Task InitializeAsync(
        IProductRepository productRepository,
        IWalletRepository walletRepository,
        IPriceRepository priceRepository)
    {
        // Initialize currencies
        var usd = new Currency("USD", "US Dollar");
        var btc = new Currency("BTC", "Bitcoin");
        var eth = new Currency("ETH", "Ethereum");
        var sol = new Currency("SOL", "Solana");
        
        // Initialize products
        var btcUsd = new Product("BTC-USD", btc, usd, 0.0001m);
        var ethUsd = new Product("ETH-USD", eth, usd, 0.001m);
        var solUsd = new Product("SOL-USD", sol, usd, 0.01m);
        
        await productRepository.AddAsync(btcUsd);
        await productRepository.AddAsync(ethUsd);
        await productRepository.AddAsync(solUsd);
        
        // Initialize prices
        await priceRepository.AddAsync(new PricePoint("BTC-USD", 75000.00m));
        await priceRepository.AddAsync(new PricePoint("ETH-USD", 4500.00m));
        await priceRepository.AddAsync(new PricePoint("SOL-USD", 195.00m));
        
        // Initialize default wallet
        var wallet = new Wallet("default", "Default Wallet");
        
        // Add some initial balances
        wallet.AddAsset(new Asset(usd, 10000.00m));
        wallet.AddAsset(new Asset(btc, 0.5m));
        wallet.AddAsset(new Asset(eth, 5.0m));
        wallet.AddAsset(new Asset(sol, 20.0m));
        
        await walletRepository.AddAsync(wallet);
    }
}