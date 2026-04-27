using Domain.Enums;

namespace Application.DTOs;

public class BicycleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public BicycleType BicycleType { get; set; }
    public string BicycleTypeName => BicycleType.ToString();
    public decimal PricePerHour { get; set; }
    public decimal PurchasePrice { get; set; }
    public bool IsAvailable { get; set; }
    public int YearManufactured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
