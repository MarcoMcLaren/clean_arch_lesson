using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

// IEntityTypeConfiguration<Bicycle> is a contract (interface) that says:
// "This class is responsible for configuring how the Bicycle entity maps to a database table."
// EF Core automatically finds and applies this class because of ApplyConfigurationsFromAssembly
// in AppDbContext — we never have to register it manually.
public class BicycleConfiguration : IEntityTypeConfiguration<Bicycle>
{
    // EF Core calls this Configure method when building the database model.
    // The 'builder' object is our tool for defining rules for the Bicycles table.
    public void Configure(EntityTypeBuilder<Bicycle> builder)
    {
        // Tell EF Core which property is the PRIMARY KEY for this table.
        // Every row in Bicycles will be uniquely identified by its Id column.
        builder.HasKey(b => b.Id);

        // .IsRequired() → this column is NOT NULL in the database (SQL will reject empty rows)
        // .HasMaxLength(100) → sets VARCHAR(100) as the column type — saves space vs TEXT
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Model)
            .IsRequired()
            .HasMaxLength(100);

        // decimal without precision causes a warning in EF Core.
        // HasPrecision(18, 2) means: up to 18 total digits, 2 after the decimal point.
        // That gives us values like 99999999999999.99 — more than enough for any price.
        builder.Property(b => b.PricePerHour)
            .HasPrecision(18, 2);

        builder.Property(b => b.PurchasePrice)
            .HasPrecision(18, 2);

        // By default EF Core would store an enum as an integer (0, 1, 2...).
        // HasConversion<string>() makes it store "Road", "Mountain" etc. instead.
        // This makes the database readable by anyone without needing to look up an enum definition.
        builder.Property(b => b.BicycleType)
            .HasConversion<string>();
    }
}
