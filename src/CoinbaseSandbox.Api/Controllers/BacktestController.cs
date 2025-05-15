namespace CoinbaseSandbox.Api.Controllers;

using System.Text.Json;
using CoinbaseSandbox.Domain.Models;
using CoinbaseSandbox.Domain.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/backtest")]
public class BacktestController : ControllerBase
{
    private readonly IBacktestService _backtestService;
    private readonly ILogger<BacktestController> _logger;

    public BacktestController(
        IBacktestService backtestService,
        ILogger<BacktestController> logger)
    {
        _backtestService = backtestService;
        _logger = logger;
    }

    public class RunBacktestRequest
    {
        public string StrategyName { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public decimal InitialBalance { get; set; } = 10000m;
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    [HttpPost("run")]
    public async Task<IActionResult> RunBacktest(
        [FromBody] RunBacktestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.StrategyName))
            {
                return BadRequest("Strategy name is required");
            }

            if (string.IsNullOrWhiteSpace(request.ProductId))
            {
                return BadRequest("Product ID is required");
            }

            if (!DateTime.TryParse(request.StartDate, out var startDate))
            {
                return BadRequest("Invalid start date format");
            }

            if (!DateTime.TryParse(request.EndDate, out var endDate))
            {
                return BadRequest("Invalid end date format");
            }

            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var result = await _backtestService.RunBacktestAsync(
                request.StrategyName,
                request.ProductId,
                startDate,
                endDate,
                request.InitialBalance,
                request.Parameters,
                cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running backtest");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpGet("strategies")]
    public async Task<IActionResult> GetStrategies(CancellationToken cancellationToken)
    {
        try
        {
            var strategies = await _backtestService.GetAvailableStrategiesAsync(cancellationToken);
            return Ok(strategies.Select(s => new
            {
                s.Name,
                s.Description,
                Parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(s.Parameters)
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strategies");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpGet("strategies/{name}")]
    public async Task<IActionResult> GetStrategy(string name, CancellationToken cancellationToken)
    {
        try
        {
            var strategy = await _backtestService.GetStrategyAsync(name, cancellationToken);
            return Ok(new
            {
                strategy.Name,
                strategy.Description,
                Parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(strategy.Parameters),
                strategy.Code
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Strategy '{name}' not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strategy {StrategyName}", name);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpPost("strategies")]
    public async Task<IActionResult> SaveStrategy(
        [FromBody] BacktestStrategy strategy,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(strategy.Name))
            {
                return BadRequest("Strategy name is required");
            }

            // Validate parameters are valid JSON
            try
            {
                var parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(strategy.Parameters);
            }
            catch
            {
                return BadRequest("Invalid parameters format");
            }

            await _backtestService.SaveStrategyAsync(strategy, cancellationToken);
            return Ok(new { success = true, message = $"Strategy '{strategy.Name}' saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving strategy {StrategyName}", strategy.Name);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpGet("results")]
    public async Task<IActionResult> GetResults(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = await _backtestService.GetBacktestResultsAsync(limit, cancellationToken);
            return Ok(results.Select(r => new
            {
                r.Id,
                r.StrategyName,
                r.ProductId,
                r.StartDate,
                r.EndDate,
                r.InitialBalance,
                r.FinalBalance,
                r.ProfitLoss,
                r.ProfitLossPercent,
                r.TotalTrades
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting backtest results");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpGet("results/{id}")]
    public async Task<IActionResult> GetResult(string id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _backtestService.GetBacktestResultAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Backtest result '{id}' not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting backtest result {ResultId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}