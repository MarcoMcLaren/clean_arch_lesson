namespace Application.DTOs;

public class RentalQuoteDto
{
    public Guid BicycleId { get; set; }
    public string BicycleName { get; set; } = string.Empty;
    public int Hours { get; set; }
    public decimal BasePrice { get; set; }
    public decimal TypeMultiplier { get; set; }
    public decimal PriceAfterTypeMultiplier { get; set; }
    public string? DiscountCode { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal FinalPrice { get; set; }
}
