using ComputerStore.Application.DTOs;

namespace ComputerStore.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<bool> UpdateAsync(int id, CreateCategoryDto dto);
    Task<bool> DeleteAsync(int id);
}

