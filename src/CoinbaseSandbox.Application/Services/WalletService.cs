namespace CoinbaseSandbox.Application.Services;

using Domain.Models;
using Domain.Repositories;
using CoinbaseSandbox.Domain.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;

    public WalletService(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Wallet> GetWalletAsync(string id, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByIdAsync(id, cancellationToken);
        if (wallet == null)
            throw new KeyNotFoundException($"Wallet {id} not found");

        return wallet;
    }

    public async Task<IEnumerable<Wallet>> GetWalletsAsync(CancellationToken cancellationToken = default)
    {
        return await _walletRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Asset> GetAssetAsync(string walletId, string currencySymbol, CancellationToken cancellationToken = default)
    {
        var wallet = await GetWalletAsync(walletId, cancellationToken);
        return wallet.GetAsset(currencySymbol);
    }

    public async Task<Asset> DepositAsync(string walletId, string currencySymbol, decimal amount, CancellationToken cancellationToken = default)
    {
        var wallet = await GetWalletAsync(walletId, cancellationToken);

        // Try to get the asset, if it doesn't exist, throw
        var asset = wallet.GetAsset(currencySymbol);

        // Perform the deposit
        asset.Deposit(amount);

        // Update the wallet
        await _walletRepository.UpdateAsync(wallet, cancellationToken);

        return asset;
    }

    public async Task<Asset> WithdrawAsync(string walletId, string currencySymbol, decimal amount, CancellationToken cancellationToken = default)
    {
        var wallet = await GetWalletAsync(walletId, cancellationToken);

        // Try to get the asset, if it doesn't exist, throw
        var asset = wallet.GetAsset(currencySymbol);

        // Perform the withdrawal
        asset.Withdraw(amount);

        // Update the wallet
        await _walletRepository.UpdateAsync(wallet, cancellationToken);

        return asset;
    }
}