namespace CoinbaseSandbox.Application.Dtos;

public record OrderDto(
    Guid Id,
    string ProductId,
    string Side,
    string Type,
    decimal Size,
    decimal? LimitPrice,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    decimal? ExecutedPrice,
    decimal? Fee
);