using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

// DbSeeder runs at startup to ensure the database has:
//   1. Migrations applied
//   2. Roles (Admin, Customer)
//   3. A default admin user
//   4. Sample bicycle data
public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        await context.Database.MigrateAsync();
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedBicyclesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@bicycleshop.com";
        if (await userManager.FindByEmailAsync(adminEmail) is not null) return;

        var admin = new ApplicationUser
        {
            FirstName = "Shop",
            LastName = "Admin",
            Email = adminEmail,
            UserName = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    private static async Task SeedBicyclesAsync(AppDbContext context)
    {
        if (await context.Bicycles.AnyAsync()) return;

        var bicycles = new List<Bicycle>
        {
            new() { Id = Guid.NewGuid(), Name = "Speedster Pro", Brand = "Trek", Model = "Domane SL5",
                    BicycleType = BicycleType.Road, PricePerHour = 12.00m, PurchasePrice = 2500m,
                    YearManufactured = 2023, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Trail Blazer", Brand = "Giant", Model = "Talon 29",
                    BicycleType = BicycleType.Mountain, PricePerHour = 15.00m, PurchasePrice = 1800m,
                    YearManufactured = 2022, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "City Glider", Brand = "Specialized", Model = "Sirrus 3.0",
                    BicycleType = BicycleType.Hybrid, PricePerHour = 10.00m, PurchasePrice = 1200m,
                    YearManufactured = 2023, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "E-Cruiser X", Brand = "Bosch", Model = "Active Line",
                    BicycleType = BicycleType.Electric, PricePerHour = 20.00m, PurchasePrice = 4500m,
                    YearManufactured = 2024, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Street Jumper", Brand = "Haro", Model = "BMX Race",
                    BicycleType = BicycleType.BMX, PricePerHour = 8.00m, PurchasePrice = 600m,
                    YearManufactured = 2022, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Alpine Crusher", Brand = "Cannondale", Model = "Trail 5",
                    BicycleType = BicycleType.Mountain, PricePerHour = 18.00m, PurchasePrice = 2200m,
                    YearManufactured = 2024, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Volt Rider", Brand = "Shimano", Model = "Steps E8000",
                    BicycleType = BicycleType.Electric, PricePerHour = 22.00m, PurchasePrice = 5000m,
                    YearManufactured = 2024, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Urban Arrow", Brand = "Scott", Model = "Sub Cross 30",
                    BicycleType = BicycleType.Hybrid, PricePerHour = 11.00m, PurchasePrice = 1400m,
                    YearManufactured = 2023, IsAvailable = true },
        };

        await context.Bicycles.AddRangeAsync(bicycles);
        await context.SaveChangesAsync();
    }
}
