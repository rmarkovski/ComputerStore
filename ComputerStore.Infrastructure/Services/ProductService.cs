using ComputerStore.Application.DTOs;
using ComputerStore.Application.Interfaces;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Categories)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                Categories = p.Categories.Select(c => c.Name).ToList()
            })
            .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            Categories = product.Categories.Select(c => c.Name).ToList()
        };
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var categories = await _context.Categories
            .Where(c => dto.Categories.Contains(c.Name))
            .ToListAsync();

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Quantity = dto.Quantity,
            Categories = categories
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = dto.Quantity,
            Categories = product.Categories.Select(c => c.Name).ToList()
        };
    }

    public async Task<bool> UpdateAsync(int id, CreateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return false;

        var categories = await _context.Categories
            .Where(c => dto.Categories.Contains(c.Name))
            .ToListAsync();

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Quantity = dto.Quantity;
        product.Categories = categories;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ImportStockAsync(List<ImportProductDto> items)
    {
        foreach (var item in items)
        {
            var categories = new List<Category>();
            foreach (var catName in item.Categories)
            {
                var name = catName.Trim();
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == name);

                if (category == null)
                {
                    category = new Category { Name = name };
                    _context.Categories.Add(category);
                }

                categories.Add(category);
            }

            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Name == item.Name);

            if (product == null)
            {
                product = new Product
                {
                    Name = item.Name,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Description = "",
                    Categories = categories
                };

                _context.Products.Add(product);
            }
            else
            {
                product.Quantity += item.Quantity;
                product.Price = item.Price;
                product.Categories = categories;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<CartDiscountResultDto> CalculateDiscountAsync(List<CartItemDto> cart)
    {
        var result = new CartDiscountResultDto();
        var productIds = cart.Select(x => x.ProductId).ToList();

        var products = await _context.Products
            .Include(p => p.Categories)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        var categoryCounts = new Dictionary<string, int>();
        decimal total = 0;
        decimal totalDiscount = 0;

        foreach (var item in cart)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
                continue;

            if (item.Quantity > product.Quantity)
            {
                return new CartDiscountResultDto
                {
                    Message = $"Not enough stock for {product.Name} (requested: {item.Quantity}, available: {product.Quantity})"
                };
            }

            decimal itemTotal = product.Price * item.Quantity;
            total += itemTotal;

            foreach (var category in product.Categories)
            {
                if (!categoryCounts.ContainsKey(category.Name))
                    categoryCounts[category.Name] = 0;

                categoryCounts[category.Name] += item.Quantity;
            }

            bool qualifies = product.Categories.Any(cat => categoryCounts[cat.Name] > 1);

            if (qualifies)
            {
                decimal discount = product.Price * 0.05m;
                totalDiscount += discount;
            }
        }

        result.TotalBeforeDiscount = total;
        result.TotalDiscount = totalDiscount;
        result.FinalPrice = total - totalDiscount;

        return result;
    }
}
