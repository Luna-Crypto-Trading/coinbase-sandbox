namespace CoinbaseSandbox.Domain.Models;

public record Currency
{
    public string Symbol { get; init; } // E.g., "BTC", "ETH", "USD"
    public string Name { get; init; } // E.g., "Bitcoin", "Ethereum", "US Dollar"
    public int DecimalPlaces { get; init; } // Precision for display and calculations

    public Currency(string symbol, string name, int decimalPlaces = 2)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (decimalPlaces < 0)
            throw new ArgumentException("Decimal places cannot be negative", nameof(decimalPlaces));

        Symbol = symbol;
        Name = name;
        DecimalPlaces = decimalPlaces;
    }
}