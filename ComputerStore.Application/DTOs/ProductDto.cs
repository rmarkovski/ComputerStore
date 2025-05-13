namespace ComputerStore.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public int Quantity { get; set; }
    public List<string> Categories { get; set; } = new();
}

