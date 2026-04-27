using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TotalCost)
            .HasPrecision(18, 2);

        builder.Property(r => r.Status)
            .HasConversion<string>();

        builder.HasOne(r => r.Bicycle)
            .WithMany(b => b.Rentals)
            .HasForeignKey(r => r.BicycleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
