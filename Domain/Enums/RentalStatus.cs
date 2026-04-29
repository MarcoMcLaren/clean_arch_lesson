namespace Domain.Enums;

// Enums are perfect for anything with a fixed, known list of states.
// A rental can only ever be Active, Completed, or Cancelled — nothing else.
// Using an enum makes that constraint explicit in the type system.
public enum RentalStatus
{
    Active,
    Completed,
    Cancelled
}
