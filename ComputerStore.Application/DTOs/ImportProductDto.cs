namespace ComputerStore.Application.DTOs;

public class ImportProductDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

