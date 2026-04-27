using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class StartRentalDto
{
    [Required]
    public Guid BicycleId { get; set; }
}
