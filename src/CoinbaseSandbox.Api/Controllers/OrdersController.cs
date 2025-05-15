namespace CoinbaseSandbox.Api.Controllers;

using Application.Dtos;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var orders = await _orderService.GetOrdersAsync(limit, cancellationToken);

        return Ok(orders.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.GetOrderAsync(id, cancellationToken);

            return Ok(MapToDto(order));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public record PlaceOrderRequest(
        string ProductId,
        string Side,
        string Type,
        decimal Size,
        decimal? LimitPrice
    );

    [HttpPost]
    public async Task<ActionResult<OrderDto>> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Parse order side
            if (!Enum.TryParse<OrderSide>(request.Side, true, out var side))
            {
                return BadRequest($"Invalid order side: {request.Side}");
            }

            // Parse order type
            if (!Enum.TryParse<OrderType>(request.Type, true, out var type))
            {
                return BadRequest($"Invalid order type: {request.Type}");
            }

            // Validate limit price for limit orders
            if (type == OrderType.Limit && !request.LimitPrice.HasValue)
            {
                return BadRequest("Limit price is required for limit orders");
            }

            var order = await _orderService.PlaceOrderAsync(
                request.ProductId,
                side,
                type,
                request.Size,
                request.LimitPrice,
                cancellationToken);

            return Ok(MapToDto(order));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<OrderDto>> CancelOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.CancelOrderAsync(id, cancellationToken);

            return Ok(MapToDto(order));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.ProductId,
            order.Side.ToString(),
            order.Type.ToString(),
            order.Size,
            order.LimitPrice,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            order.ExecutedPrice,
            order.Fee
        );
    }
}