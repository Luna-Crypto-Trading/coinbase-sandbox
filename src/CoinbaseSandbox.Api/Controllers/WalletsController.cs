namespace CoinbaseSandbox.Api.Controllers;

using Application.Dtos; 
using Domain.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/wallets")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;
    
    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WalletDto>>> GetWallets(CancellationToken cancellationToken)
    {
        var wallets = await _walletService.GetWalletsAsync(cancellationToken);
        
        return Ok(wallets.Select(w => new WalletDto(
            w.Id,
            w.Name,
            w.Assets.Select(a => new AssetDto(
                a.Currency.Symbol,
                a.Currency.Name,
                a.Balance
            ))
        )));
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<WalletDto>> GetWallet(string id, CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await _walletService.GetWalletAsync(id, cancellationToken);
            
            return Ok(new WalletDto(
                wallet.Id,
                wallet.Name,
                wallet.Assets.Select(a => new AssetDto(
                    a.Currency.Symbol,
                    a.Currency.Name,
                    a.Balance
                ))
            ));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
    
    [HttpGet("{id}/assets/{symbol}")]
    public async Task<ActionResult<AssetDto>> GetAsset(string id, string symbol, CancellationToken cancellationToken)
    {
        try
        {
            var asset = await _walletService.GetAssetAsync(id, symbol, cancellationToken);
            
            return Ok(new AssetDto(
                asset.Currency.Symbol,
                asset.Currency.Name,
                asset.Balance
            ));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
    
    // This would simulate depositing funds in a sandbox environment
    [HttpPost("{id}/assets/{symbol}/deposit")]
    public async Task<ActionResult<AssetDto>> Deposit(
        string id, 
        string symbol, 
        [FromBody] decimal amount, 
        CancellationToken cancellationToken)
    {
        try
        {
            var asset = await _walletService.DepositAsync(id, symbol, amount, cancellationToken);
            
            return Ok(new AssetDto(
                asset.Currency.Symbol,
                asset.Currency.Name,
                asset.Balance
            ));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // This would simulate withdrawing funds in a sandbox environment
    [HttpPost("{id}/assets/{symbol}/withdraw")]
    public async Task<ActionResult<AssetDto>> Withdraw(
        string id, 
        string symbol, 
        [FromBody] decimal amount, 
        CancellationToken cancellationToken)
    {
        try
        {
            var asset = await _walletService.WithdrawAsync(id, symbol, amount, cancellationToken);
            
            return Ok(new AssetDto(
                asset.Currency.Symbol,
                asset.Currency.Name,
                asset.Balance
            ));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
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
}