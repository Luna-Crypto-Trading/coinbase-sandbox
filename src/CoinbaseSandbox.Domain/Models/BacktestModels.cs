namespace CoinbaseSandbox.Domain.Models;

public class BacktestStrategy
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
}

public class BacktestResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string StrategyName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public decimal FinalBalance { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal ProfitLossPercent { get; set; }
    public int TotalTrades { get; set; }
    public List<BacktestTrade> Trades { get; set; } = new List<BacktestTrade>();
    public List<BacktestDataPoint> EquityCurve { get; set; } = new List<BacktestDataPoint>();
}

public class BacktestTrade
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; }
    public OrderSide Side { get; set; }
    public decimal Price { get; set; }
    public decimal Size { get; set; }
    public decimal Value { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class BacktestDataPoint
{
    public DateTime Timestamp { get; set; }
    public decimal Price { get; set; }
    public decimal PortfolioValue { get; set; }
}