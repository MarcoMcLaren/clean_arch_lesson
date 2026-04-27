using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class UpdateBicycleDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    public BicycleType BicycleType { get; set; }

    [Range(0.01, 10000)]
    public decimal PricePerHour { get; set; }

    [Range(0.01, 100000)]
    public decimal PurchasePrice { get; set; }

    public bool IsAvailable { get; set; }

    [Range(1900, 2100)]
    public int YearManufactured { get; set; }
}
