using System.Net;
using System.Net.Http.Json;
using ComputerStore.API;
using ComputerStore.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;


namespace ComputerStore.Tests
{
    public class ProductControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProductControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsSuccess()
        {
            var response = await _client.GetAsync("/api/Product");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Create_ReturnsCreatedProduct()
        {
            var product = new CreateProductDto
            {
                Name = "Integration CPU",
                Description = "Test CPU",
                Price = 299.99m,
                Quantity = 10,
                Categories = new List<string> { "CPU" }
            };

            var response = await _client.PostAsJsonAsync("/api/Product", product);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<ProductDto>();
            Assert.NotNull(created);
            Assert.Equal("Integration CPU", created.Name);
        }

        [Fact]
        public async Task Update_ReturnsNoContent()
        {
            var product = new CreateProductDto
            {
                Name = "CPU Update",
                Description = "Original",
                Price = 100,
                Quantity = 1,
                Categories = new List<string> { "CPU" }
            };

            var createResp = await _client.PostAsJsonAsync("/api/Product", product);
            var created = await createResp.Content.ReadFromJsonAsync<ProductDto>();

            var update = new CreateProductDto
            {
                Name = "CPU Updated",
                Description = "Updated",
                Price = 200,
                Quantity = 5,
                Categories = new List<string> { "CPU" }
            };

            var response = await _client.PutAsJsonAsync($"/api/Product/{created.Id}", update);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_RemovesProduct()
        {
            var product = new CreateProductDto
            {
                Name = "To Delete",
                Description = "Temp",
                Price = 100,
                Quantity = 1,
                Categories = new List<string> { "CPU" }
            };

            var createResp = await _client.PostAsJsonAsync("/api/Product", product);
            var created = await createResp.Content.ReadFromJsonAsync<ProductDto>();

            var deleteResp = await _client.DeleteAsync($"/api/Product/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);
        }

        [Fact]
        public async Task ImportStock_WorksCorrectly()
        {
            var stock = new List<ImportProductDto>
            {
                new ImportProductDto
                {
                    Name = "Imported CPU",
                    Price = 149.99m,
                    Quantity = 20,
                    Categories = new List<string> { "CPU" }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/Product/import", stock);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task CalculateDiscount_ReturnsCorrectDiscount()
        {
            var product = new CreateProductDto
            {
                Name = "Discount CPU",
                Description = "Discountable",
                Price = 100,
                Quantity = 10,
                Categories = new List<string> { "CPU" }
            };

            var createResp = await _client.PostAsJsonAsync("/api/Product", product);
            var created = await createResp.Content.ReadFromJsonAsync<ProductDto>();

            var cart = new List<CartItemDto>
            {
                new CartItemDto
                {
                    ProductId = created.Id,
                    Quantity = 2
                }
            };

            var discountResp = await _client.PostAsJsonAsync("/api/Product/calculate-discount", cart);
            discountResp.EnsureSuccessStatusCode();

            var result = await discountResp.Content.ReadFromJsonAsync<CartDiscountResultDto>();
            Assert.NotNull(result);
            Assert.True(result.TotalDiscount > 0);
        }
    }
}
