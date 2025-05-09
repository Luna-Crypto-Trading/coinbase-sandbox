namespace CoinbaseSandbox.Domain.Services;

using Models;

public interface IWalletService
{
    Task<Wallet> GetWalletAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Wallet>> GetWalletsAsync(CancellationToken cancellationToken = default);
    Task<Asset> GetAssetAsync(string walletId, string currencySymbol, CancellationToken cancellationToken = default);
    Task<Asset> DepositAsync(string walletId, string currencySymbol, decimal amount, CancellationToken cancellationToken = default);
    Task<Asset> WithdrawAsync(string walletId, string currencySymbol, decimal amount, CancellationToken cancellationToken = default);
}