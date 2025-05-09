using CoinbaseSandbox.Application.Dtos;

namespace CoinbaseSandbox.Api.Controllers;

using Application.Dtos;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(CancellationToken cancellationToken)
    {
        var products = await _productService.GetProductsAsync(cancellationToken);
        
        return Ok(products.Select(p => new ProductDto(
            p.Id,
            p.BaseCurrency.Symbol,
            p.BaseCurrency.Name,
            p.QuoteCurrency.Symbol,
            p.QuoteCurrency.Name,
            p.MinimumOrderSize
        )));
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(string id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.GetProductAsync(id, cancellationToken);
            
            return Ok(new ProductDto(
                product.Id,
                product.BaseCurrency.Symbol,
                product.BaseCurrency.Name,
                product.QuoteCurrency.Symbol,
                product.QuoteCurrency.Name,
                product.MinimumOrderSize
            ));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}