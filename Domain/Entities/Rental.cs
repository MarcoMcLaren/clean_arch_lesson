using Domain.Enums;

namespace Domain.Entities;

// Rental represents a single rental transaction — one customer, one bike, one time period.
// This entity joins together a Bicycle and a User (from ASP.NET Identity).
public class Rental
{
    public Guid Id { get; set; }

    // Foreign Key — this column in the Rentals table stores the Id of the Bicycle being rented.
    // EF Core uses this Guid to JOIN the Bicycles table when we ask for rental.Bicycle.
    public Guid BicycleId { get; set; }

    // UserId comes from ASP.NET Identity — it's the Id of the logged-in user who made the rental.
    // We store it as a string because Identity uses string keys by default.
    public string UserId { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    // The ? makes EndTime nullable — it has no value until the rental is actually completed.
    // A null EndTime means "this rental is still in progress".
    public DateTime? EndTime { get; set; }

    // Also nullable — TotalCost is only calculated when the rental ends.
    public decimal? TotalCost { get; set; }

    // Defaults to Active because every rental starts as active.
    public RentalStatus Status { get; set; } = RentalStatus.Active;

    // Navigation property — the "other side" of the relationship.
    // This lets us write rental.Bicycle.Name in our code instead of doing a manual lookup.
    // null! tells the compiler "I know this looks null, but EF Core will always populate it".
    public Bicycle Bicycle { get; set; } = null!;
}
