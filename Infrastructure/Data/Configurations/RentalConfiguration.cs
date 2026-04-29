using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

// Same pattern as BicycleConfiguration — one configuration class per entity.
// Keeping these in separate files means each file has a single responsibility
// and is easy to find when you need to change a column rule.
public class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        builder.HasKey(r => r.Id);

        // TotalCost is nullable (decimal?) but we still need to set precision
        // for when it does have a value, to avoid floating-point storage issues.
        builder.Property(r => r.TotalCost)
            .HasPrecision(18, 2);

        // Store the RentalStatus enum as "Active", "Completed", "Cancelled" in the DB
        // — same reasoning as BicycleType above.
        builder.Property(r => r.Status)
            .HasConversion<string>();

        // This is where we define the RELATIONSHIP between the two tables:
        //   HasOne   → a Rental has ONE Bicycle
        //   WithMany → that Bicycle can have MANY Rentals
        //   HasForeignKey → the BicycleId column in the Rentals table is the foreign key
        //
        // Result in SQL:
        //   FOREIGN KEY (BicycleId) REFERENCES Bicycles(Id)
        builder.HasOne(r => r.Bicycle)
            .WithMany(b => b.Rentals)
            .HasForeignKey(r => r.BicycleId)
            // Restrict means: you CANNOT delete a Bicycle that still has Rentals.
            // This protects data integrity — we never want orphaned rental records.
            .OnDelete(DeleteBehavior.Restrict);
    }
}
