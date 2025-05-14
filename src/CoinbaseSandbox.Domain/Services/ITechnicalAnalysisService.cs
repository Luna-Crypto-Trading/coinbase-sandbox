namespace CoinbaseSandbox.Domain.Services;

public interface ITechnicalAnalysisService
{
    Task<Dictionary<string, decimal>> CalculateSimpleMovingAveragesAsync(
        string productId, 
        int[] periods, 
        CancellationToken cancellationToken = default);
        
    Task<Dictionary<string, decimal>> CalculateExponentialMovingAveragesAsync(
        string productId, 
        int[] periods, 
        CancellationToken cancellationToken = default);
        
    Task<decimal> CalculateRelativeStrengthIndexAsync(
        string productId, 
        int period = 14, 
        CancellationToken cancellationToken = default);
        
    Task<(decimal upper, decimal middle, decimal lower)> CalculateBollingerBandsAsync(
        string productId, 
        int period = 20, 
        double standardDeviations = 2.0, 
        CancellationToken cancellationToken = default);
        
    Task<Dictionary<string, decimal>> GetTechnicalIndicatorsAsync(
        string productId, 
        CancellationToken cancellationToken = default);
}