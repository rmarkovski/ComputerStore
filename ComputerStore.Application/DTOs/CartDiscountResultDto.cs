namespace ComputerStore.Application.DTOs;

public class CartDiscountResultDto
{
    public decimal TotalBeforeDiscount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal FinalPrice { get; set; }
    public string? Message { get; set; }
}
