using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Data;

// DbContext is the EF Core class that acts as the bridge between your C# code and the database.
// Think of it as the "session" — everything you do with the database goes through here.
//
// We inherit from IdentityDbContext<ApplicationUser> instead of plain DbContext
// because ASP.NET Identity needs its own tables (AspNetUsers, AspNetRoles, etc.).
// IdentityDbContext adds all of those automatically on top of our own tables.
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    // The options object tells EF Core which database to connect to (SQL Server, SQLite, etc.)
    // and the connection string. This is wired up in Program.cs via dependency injection.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet<T> represents a TABLE in the database.
    // DbSet<Bicycle> = the Bicycles table. DbSet<Rental> = the Rentals table.
    // We use these in repositories to query and save data: _context.Bicycles.ToListAsync()
    public DbSet<Bicycle> Bicycles => Set<Bicycle>();
    public DbSet<Rental> Rentals => Set<Rental>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must call base first — this lets IdentityDbContext set up its own tables
        // (AspNetUsers, AspNetRoles, etc.) before we add ours.
        base.OnModelCreating(builder);

        // Instead of configuring every entity inline here (which gets messy fast),
        // we scan the Infrastructure assembly and automatically find every class
        // that implements IEntityTypeConfiguration<T> — e.g. BicycleConfiguration.
        // Each entity gets its own clean configuration file.
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    // We override SaveChangesAsync so that CreatedAt and UpdatedAt are always
    // set by the server — no developer needs to remember to set them manually.
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // ChangeTracker knows which entities EF Core is currently tracking (watching for changes).
        // We filter to only Bicycle entries — Rental doesn't have audit timestamps.
        var entries = ChangeTracker.Entries<Bicycle>();

        foreach (var entry in entries)
        {
            // EntityState.Added = this is a brand new row being INSERTed
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;

            // EntityState.Modified = an existing row being UPDATEd
            // We always update UpdatedAt on both insert AND update
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        // Call the real SaveChangesAsync — this is what actually runs the SQL
        return base.SaveChangesAsync(cancellationToken);
    }
}
