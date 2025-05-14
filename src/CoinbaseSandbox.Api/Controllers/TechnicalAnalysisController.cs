namespace CoinbaseSandbox.Api.Controllers;

using CoinbaseSandbox.Domain.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/technical-analysis")]
public class TechnicalAnalysisController : ControllerBase
{
    private readonly ITechnicalAnalysisService _technicalAnalysisService;
    private readonly ILogger<TechnicalAnalysisController> _logger;
    
    public TechnicalAnalysisController(
        ITechnicalAnalysisService technicalAnalysisService,
        ILogger<TechnicalAnalysisController> logger)
    {
        _technicalAnalysisService = technicalAnalysisService;
        _logger = logger;
    }
    
    [HttpGet("{productId}/indicators")]
    public async Task<IActionResult> GetIndicators(
        string productId,
        CancellationToken cancellationToken)
    {
        try
        {
            var indicators = await _technicalAnalysisService.GetTechnicalIndicatorsAsync(
                productId, 
                cancellationToken);
                
            return Ok(indicators);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Product '{productId}' not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting technical indicators for {ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
    
    [HttpGet("{productId}/sma")]
    public async Task<IActionResult> GetSimpleMovingAverages(
        string productId,
        [FromQuery] string periods = "10,20,50,200",
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse periods
            var periodValues = periods.Split(',')
                .Select(p => int.TryParse(p.Trim(), out var result) ? result : 0)
                .Where(p => p > 0)
                .ToArray();
                
            if (!periodValues.Any())
            {
                return BadRequest(new { error = "Invalid periods. Provide comma-separated integers." });
            }
            
            var smas = await _technicalAnalysisService.CalculateSimpleMovingAveragesAsync(
                productId, 
                periodValues, 
                cancellationToken);
                
            return Ok(smas);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Product '{productId}' not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating SMAs for {ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
    
    [HttpGet("{productId}/ema")]
    public async Task<IActionResult> GetExponentialMovingAverages(
        string productId,
        [FromQuery] string periods = "9,12,26",
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse periods
            var periodValues = periods.Split(',')
                .Select(p => int.TryParse(p.Trim(), out var result) ? result : 0)
                .Where(p => p > 0)
                .ToArray();
                
            if (!periodValues.Any())
            {
                return BadRequest(new { error = "Invalid periods. Provide comma-separated integers." });
            }
            
            var emas = await _technicalAnalysisService.CalculateExponentialMovingAveragesAsync(
                productId, 
                periodValues, 
                cancellationToken);
                
            return Ok(emas);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Product '{productId}' not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating EMAs for {ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
    
    [HttpGet("{productId}/rsi")]
    public async Task<IActionResult> GetRelativeStrengthIndex(
        string productId,
        [FromQuery] int period = 14,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (period <= 0)
            {
                return BadRequest(new { error = "Period must be positive" });
            }
            
            var rsi = await _technicalAnalysisService.CalculateRelativeStrengthIndexAsync(
                productId, 
                period, 
                cancellationToken);
                
            return Ok(new { rsi });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Product '{productId}' not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating RSI for {ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
    
    [HttpGet("{productId}/bollinger-bands")]
    public async Task<IActionResult> GetBollingerBands(
        string productId,
        [FromQuery] int period = 20,
        [FromQuery] double standardDeviations = 2.0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (period <= 0)
            {
                return BadRequest(new { error = "Period must be positive" });
            }
            
            if (standardDeviations <= 0)
            {
                return BadRequest(new { error = "Standard deviations must be positive" });
            }
            
            var bollingerBands = await _technicalAnalysisService.CalculateBollingerBandsAsync(
                productId, 
                period, 
                standardDeviations, 
                cancellationToken);
                
            return Ok(new
            {
                upper = bollingerBands.upper,
                middle = bollingerBands.middle,
                lower = bollingerBands.lower
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Product '{productId}' not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Bollinger Bands for {ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}