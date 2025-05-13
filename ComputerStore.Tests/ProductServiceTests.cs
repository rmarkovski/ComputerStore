using Microsoft.EntityFrameworkCore;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Data;
using ComputerStore.Infrastructure.Services;
using ComputerStore.Application.DTOs;

namespace ComputerStore.Tests
{
    public class ProductServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            using var context = GetDbContext();
            context.Products.AddRange(new Product { Name = "P1", Price = 10, Quantity = 2 }, new Product { Name = "P2", Price = 20, Quantity = 3 });
            await context.SaveChangesAsync();
            var service = new ProductService(context);
            var result = await service.GetAllAsync();
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectProduct()
        {
            using var context = GetDbContext();
            var p = new Product { Name = "Test", Price = 100, Quantity = 1 };
            context.Products.Add(p);
            await context.SaveChangesAsync();
            var service = new ProductService(context);
            var result = await service.GetByIdAsync(p.Id);
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullIfNotFound()
        {
            using var context = GetDbContext();
            var service = new ProductService(context);
            var result = await service.GetByIdAsync(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_AddsProductCorrectly()
        {
            using var context = GetDbContext();
            context.Categories.Add(new Category { Name = "CPU" });
            await context.SaveChangesAsync();
            var service = new ProductService(context);

            var dto = new CreateProductDto
            {
                Name = "New Product",
                Description = "Desc",
                Price = 99,
                Quantity = 5,
                Categories = new List<string> { "CPU" }
            };

            var result = await service.CreateAsync(dto);
            Assert.Equal("New Product", result.Name);
            Assert.Equal(99, result.Price);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingProduct()
        {
            using var context = GetDbContext();
            var product = new Product { Name = "Old", Price = 20, Quantity = 1 };
            context.Products.Add(product);
            context.Categories.Add(new Category { Name = "UpdatedCategory" });
            await context.SaveChangesAsync();

            var dto = new CreateProductDto
            {
                Name = "Updated",
                Description = "UpdatedDesc",
                Price = 40,
                Quantity = 10,
                Categories = new List<string> { "UpdatedCategory" }
            };

            var service = new ProductService(context);
            var result = await service.UpdateAsync(product.Id, dto);

            Assert.True(result);
            var updated = await context.Products.Include(p => p.Categories).FirstAsync(p => p.Id == product.Id);
            Assert.Equal("Updated", updated.Name);
            Assert.Equal(10, updated.Quantity);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFalseIfNotFound()
        {
            using var context = GetDbContext();
            var dto = new CreateProductDto { Name = "Nothing", Price = 1, Quantity = 1, Categories = new List<string>() };
            var service = new ProductService(context);
            var result = await service.UpdateAsync(1234, dto);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_RemovesProduct()
        {
            using var context = GetDbContext();
            var p = new Product { Name = "ToDelete", Price = 10, Quantity = 1 };
            context.Products.Add(p);
            await context.SaveChangesAsync();

            var service = new ProductService(context);
            var result = await service.DeleteAsync(p.Id);
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalseIfNotFound()
        {
            using var context = GetDbContext();
            var service = new ProductService(context);
            var result = await service.DeleteAsync(12345);
            Assert.False(result);
        }

        [Fact]
        public async Task ImportStockAsync_AddsNewAndUpdatesExisting()
        {
            using var context = GetDbContext();
            context.Categories.Add(new Category { Name = "Cat1" });
            context.Products.Add(new Product { Name = "Existing", Price = 10, Quantity = 5 });
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            var import = new List<ImportProductDto>
            {
                new ImportProductDto { Name = "Existing", Price = 10, Quantity = 10, Categories = new List<string> { "Cat1" } },
                new ImportProductDto { Name = "New", Price = 50, Quantity = 5, Categories = new List<string> { "Cat1" } }
            };

            await service.ImportStockAsync(import);

            var existing = await context.Products.FirstOrDefaultAsync(p => p.Name == "Existing");
            var added = await context.Products.FirstOrDefaultAsync(p => p.Name == "New");

            Assert.Equal(15, existing.Quantity);
            Assert.NotNull(added);
        }

        [Fact]
        public async Task CalculateDiscountAsync_ReturnsCorrectDiscount()
        {
            using var context = GetDbContext();
            var category = new Category { Name = "TestCat" };
            var product = new Product { Name = "P1", Price = 100, Quantity = 10, Categories = new List<Category> { category } };
            var product2 = new Product { Name = "P2", Price = 100, Quantity = 10, Categories = new List<Category> { category } };
            context.Products.AddRange(product, product2);
            await context.SaveChangesAsync();

            var cart = new List<CartItemDto>
            {
                new CartItemDto { ProductId = product.Id, Quantity = 1 },
                new CartItemDto { ProductId = product2.Id, Quantity = 1 }
            };

            var service = new ProductService(context);
            var result = await service.CalculateDiscountAsync(cart);
            Assert.Equal(200, result.TotalBeforeDiscount);
            Assert.True(result.TotalDiscount > 0);
        }

        [Fact]
        public async Task CalculateDiscountAsync_ReturnsMessageIfNotEnoughStock()
        {
            using var context = GetDbContext();
            var product = new Product { Name = "LowStock", Price = 100, Quantity = 1 };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var cart = new List<CartItemDto>
            {
                new CartItemDto { ProductId = product.Id, Quantity = 5 }
            };

            var service = new ProductService(context);
            var result = await service.CalculateDiscountAsync(cart);

            Assert.NotNull(result.Message);
        }
    }
}
