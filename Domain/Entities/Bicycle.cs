using Domain.Enums;

namespace Domain.Entities;

// A class is a blueprint. Just like a blueprint for a house describes what every house
// built from it will have (rooms, doors, windows), this class describes what every
// Bicycle object in our system will have.

// This is also called an "Entity" — it represents a real-world thing that has
// an identity (it has an Id) and is stored in the database.
public class Bicycle
{
    // Guid (Globally Unique Identifier) — a unique ID like "3f2504e0-4f89-11d3-9a0c-0305e82c3301"
    // EF Core uses this as the PRIMARY KEY in the Bicycles table.
    public Guid Id { get; set; }

    // { get; set; } means this property can be read and written from outside the class.
    // = string.Empty ensures we never start with a null string — safer default.
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    // Using our enum type here means this property can only ever hold a valid BicycleType value.
    // EF Core will store this as a string ("Road", "Mountain" etc.) because of our configuration.
    public BicycleType BicycleType { get; set; }

    // decimal is the right type for money — it avoids the floating-point rounding errors
    // that come with float or double (e.g., 0.1 + 0.2 != 0.3 with floats).
    public decimal PricePerHour { get; set; }
    public decimal PurchasePrice { get; set; }

    // Simple bool — true means the bike is available to rent right now.
    // Defaults to true because a newly added bike should be available immediately.
    public bool IsAvailable { get; set; } = true;

    public int YearManufactured { get; set; }

    // These two are set automatically in AppDbContext.SaveChangesAsync —
    // we never have to set them manually in business logic.
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property — this is how EF Core models the ONE-TO-MANY relationship.
    // One Bicycle can have MANY Rentals. EF Core uses this to build the foreign key
    // in the Rentals table (BicycleId column points back to this Bicycle).
    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
