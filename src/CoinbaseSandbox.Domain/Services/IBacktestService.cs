namespace CoinbaseSandbox.Domain.Services;

using Models;

public interface IBacktestService
{
    Task<BacktestResult> RunBacktestAsync(
        string strategyName,
        string productId,
        DateTime startDate,
        DateTime endDate,
        decimal initialBalance,
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken = default);
        
    Task<IEnumerable<BacktestStrategy>> GetAvailableStrategiesAsync(
        CancellationToken cancellationToken = default);
        
    Task<BacktestStrategy> GetStrategyAsync(
        string name, 
        CancellationToken cancellationToken = default);
        
    Task<BacktestResult> GetBacktestResultAsync(
        string id, 
        CancellationToken cancellationToken = default);
        
    Task<IEnumerable<BacktestResult>> GetBacktestResultsAsync(
        int limit = 10, 
        CancellationToken cancellationToken = default);
        
    Task SaveStrategyAsync(
        BacktestStrategy strategy, 
        CancellationToken cancellationToken = default);
}