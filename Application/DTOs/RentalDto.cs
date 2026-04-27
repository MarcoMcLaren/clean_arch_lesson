using Domain.Enums;

namespace Application.DTOs;

public class RentalDto
{
    public Guid Id { get; set; }
    public Guid BicycleId { get; set; }
    public string BicycleName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? TotalCost { get; set; }
    public RentalStatus Status { get; set; }
    public string StatusName => Status.ToString();
}
