namespace CoinbaseSandbox.Infrastructure.Repositories;

using System.Collections.Concurrent;
using Domain.Models;
using CoinbaseSandbox.Domain.Repositories;

public class InMemoryWalletRepository : IWalletRepository
{
    private readonly ConcurrentDictionary<string, Wallet> _wallets = new();
    
    public Task<Wallet?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _wallets.TryGetValue(id, out var wallet);
        return Task.FromResult(wallet);
    }
    
    public Task<IEnumerable<Wallet>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Wallet>>(_wallets.Values.ToList());
    }
    
    public Task<Wallet> AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        _wallets[wallet.Id] = wallet;
        return Task.FromResult(wallet);
    }
    
    public Task<Wallet> UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        _wallets[wallet.Id] = wallet;
        return Task.FromResult(wallet);
    }
}