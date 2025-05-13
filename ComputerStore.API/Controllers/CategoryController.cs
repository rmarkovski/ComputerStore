using Microsoft.AspNetCore.Mvc;
using ComputerStore.Application.DTOs;
using ComputerStore.Application.Interfaces;

namespace ComputerStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound(new { message = "Category not found." });

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Name is required." });

        var created = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateCategoryDto dto)
    {
        var success = await _categoryService.UpdateAsync(id, dto);
        if (!success)
            return NotFound(new { message = "Category not found." });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _categoryService.DeleteAsync(id);
        if (!success)
            return NotFound(new { message = "Category not found." });

        return NoContent();
    }
}
