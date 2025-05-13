using Microsoft.AspNetCore.Mvc;
using ComputerStore.Application.DTOs;
using ComputerStore.Application.Interfaces;

namespace ComputerStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Product not found." });

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        _logger.LogInformation("Received CreateProductDto: {@Dto}", dto);

        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Price <= 0)
            return BadRequest(new { message = "Product name and price are required." });

        var created = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateProductDto dto)
    {
        var success = await _productService.UpdateAsync(id, dto);
        if (!success)
            return NotFound(new { message = "Product not found." });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _productService.DeleteAsync(id);
        if (!success)
            return NotFound(new { message = "Product not found." });

        return NoContent();
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportStock([FromBody] List<ImportProductDto> importItems)
    {
        if (importItems == null || importItems.Count == 0)
            return BadRequest(new { message = "No data provided." });

        await _productService.ImportStockAsync(importItems);
        return Ok(new { message = "Stock imported successfully." });
    }

    [HttpPost("calculate-discount")]
    public async Task<IActionResult> CalculateDiscount([FromBody] List<CartItemDto> cart)
    {
        if (cart == null || cart.Count == 0)
            return BadRequest(new { message = "Cart is empty." });

        var result = await _productService.CalculateDiscountAsync(cart);

        if (!string.IsNullOrEmpty(result.Message))
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }
}
