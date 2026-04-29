namespace Domain.Enums;

// An enum is a named set of constant values.
// Instead of storing magic numbers (0, 1, 2...) in code, we give each option a meaningful name.
// This makes the code read like English and prevents invalid values from ever being used.
public enum BicycleType
{
    Road,
    Mountain,
    Hybrid,
    Electric,
    BMX
}
