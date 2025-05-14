namespace CoinbaseSandbox.Api.Controllers;

using CoinbaseSandbox.Domain.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;
    
    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }
    
    public class SubscribeToPriceAlertsRequest
    {
        public string ProductId { get; set; } = string.Empty;
        public decimal Threshold { get; set; } = 1.0m; // Default to 1% threshold
    }
    
    [HttpPost("subscribe/price-alerts")]
    public async Task<IActionResult> SubscribeToPriceAlerts(
        [FromBody] SubscribeToPriceAlertsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ProductId))
            {
                return BadRequest("Product ID is required");
            }
            
            if (request.Threshold <= 0)
            {
                return BadRequest("Threshold must be positive");
            }
            
            await _notificationService.SubscribeToPriceAlertsAsync(
                request.ProductId, 
                request.Threshold, 
                cancellationToken);
                
            return Ok(new
            {
                success = true,
                message = $"Subscribed to price alerts for {request.ProductId} with threshold {request.Threshold}%"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to price alerts");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
    
    [HttpDelete("unsubscribe/price-alerts/{productId}")]
    public async Task<IActionResult> UnsubscribeFromPriceAlerts(
        string productId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.UnsubscribeFromPriceAlertsAsync(
                productId, 
                cancellationToken);
                
            return Ok(new
            {
                success = true,
                message = $"Unsubscribed from price alerts for {productId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from price alerts");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}