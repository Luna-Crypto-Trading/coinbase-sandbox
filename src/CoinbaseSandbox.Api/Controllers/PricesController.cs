namespace CoinbaseSandbox.Api.Controllers;

using Application.Dtos;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/prices")]
public class PricesController : ControllerBase
{
    private readonly IPriceService _priceService;

    public PricesController(IPriceService priceService)
    {
        _priceService = priceService;
    }

    [HttpGet("{productId}/current")]
    public async Task<ActionResult<decimal>> GetCurrentPrice(
        string productId,
        CancellationToken cancellationToken)
    {
        try
        {
            var price = await _priceService.GetCurrentPriceAsync(productId, cancellationToken);

            return Ok(price);
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

    [HttpGet("{productId}/history")]
    public async Task<ActionResult<IEnumerable<PriceDto>>> GetPriceHistory(
        string productId,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        CancellationToken cancellationToken)
    {
        try
        {
            var priceHistory = await _priceService.GetPriceHistoryAsync(
                productId,
                start,
                end,
                cancellationToken);

            return Ok(priceHistory.Select(p => new PriceDto(
                p.ProductId,
                p.Price,
                p.Timestamp
            )));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // This would only be available in the sandbox environment for testing
    [HttpPost("{productId}/mock")]
    public async Task<ActionResult<PriceDto>> SetMockPrice(
        string productId,
        [FromBody] decimal price,
        CancellationToken cancellationToken)
    {
        try
        {
            var pricePoint = await _priceService.SetMockPriceAsync(
                productId,
                price,
                cancellationToken);

            return Ok(new PriceDto(
                pricePoint.ProductId,
                pricePoint.Price,
                pricePoint.Timestamp
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}