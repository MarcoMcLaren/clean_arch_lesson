using Domain.Enums;

namespace Domain.Entities;

public class Bicycle
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public BicycleType BicycleType { get; set; }
    public decimal PricePerHour { get; set; }
    public decimal PurchasePrice { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int YearManufactured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
