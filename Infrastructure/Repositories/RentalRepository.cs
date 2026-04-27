using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Rental?> GetByIdAsync(Guid id)
    {
        return await _context.Rentals.Include(r => r.Bicycle)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Rental>> GetActiveAsync()
    {
        return await _context.Rentals.Include(r => r.Bicycle).AsNoTracking()
            .Where(r => r.Status == RentalStatus.Active)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rental>> GetByBicycleIdAsync(Guid bicycleId)
    {
        return await _context.Rentals.Include(r => r.Bicycle).AsNoTracking()
            .Where(r => r.BicycleId == bicycleId)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();
    }

    public async Task AddAsync(Rental rental)
    {
        await _context.Rentals.AddAsync(rental);
    }

    public Task UpdateAsync(Rental rental)
    {
        _context.Rentals.Update(rental);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
