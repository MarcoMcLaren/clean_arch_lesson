using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class BicycleConfiguration : IEntityTypeConfiguration<Bicycle>
{
    public void Configure(EntityTypeBuilder<Bicycle> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.PricePerHour)
            .HasPrecision(18, 2);

        builder.Property(b => b.PurchasePrice)
            .HasPrecision(18, 2);

        // Store enum as its string name in the DB column — easier to read in SQL queries
        builder.Property(b => b.BicycleType)
            .HasConversion<string>();
    }
}
