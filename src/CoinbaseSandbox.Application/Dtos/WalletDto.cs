namespace CoinbaseSandbox.Application.Dtos;

public record AssetDto(string CurrencySymbol, string CurrencyName, decimal Balance);

public record WalletDto(string Id, string Name, IEnumerable<AssetDto> Assets);