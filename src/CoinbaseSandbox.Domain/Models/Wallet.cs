namespace CoinbaseSandbox.Domain.Models;

public class Wallet
{
    private readonly Dictionary<string, Asset> _assets = new();
    public string Id { get; init; }
    public string Name { get; init; }

    public Wallet(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public IReadOnlyCollection<Asset> Assets => _assets.Values.ToList().AsReadOnly();

    public Asset GetAsset(string currencySymbol)
    {
        if (_assets.TryGetValue(currencySymbol, out var asset))
            return asset;

        throw new KeyNotFoundException($"Asset {currencySymbol} not found in wallet");
    }

    public void AddAsset(Asset asset)
    {
        var symbol = asset.Currency.Symbol;
        if (_assets.ContainsKey(symbol))
            throw new InvalidOperationException($"Asset {symbol} already exists in wallet");

        _assets[symbol] = asset;
    }

    public bool TryGetAsset(string currencySymbol, out Asset? asset)
    {
        return _assets.TryGetValue(currencySymbol, out asset);
    }
}