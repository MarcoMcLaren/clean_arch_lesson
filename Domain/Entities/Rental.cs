using Domain.Enums;

namespace Domain.Entities;

public class Rental
{
    public Guid Id { get; set; }
    public Guid BicycleId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? TotalCost { get; set; }
    public RentalStatus Status { get; set; } = RentalStatus.Active;

    public Bicycle Bicycle { get; set; } = null!;
}
