namespace CoinbaseSandbox.Application.Dtos;

public record PriceDto(
    string ProductId,
    decimal Price,
    DateTime Timestamp
);