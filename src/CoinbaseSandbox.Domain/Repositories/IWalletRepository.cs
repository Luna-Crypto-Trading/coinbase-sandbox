namespace CoinbaseSandbox.Domain.Repositories;

using Models;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Wallet>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Wallet> AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
    Task<Wallet> UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default);
}