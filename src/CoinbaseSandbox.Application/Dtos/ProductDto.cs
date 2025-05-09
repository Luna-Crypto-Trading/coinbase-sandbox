namespace CoinbaseSandbox.Application.Dtos;

public record ProductDto(
    string Id,
    string BaseCurrencySymbol,
    string BaseCurrencyName,
    string QuoteCurrencySymbol,
    string QuoteCurrencyName,
    decimal MinimumOrderSize
);