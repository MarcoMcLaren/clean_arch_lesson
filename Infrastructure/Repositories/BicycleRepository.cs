using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

// BicycleRepository implements the IBicycleRepository interface defined in Domain.
// This is the only class in the solution that talks to EF Core for bicycles.
// The Application layer and controllers never see AppDbContext directly.

//BicycleRepository implements IBicycleRepository
//"Implement" means: write the actual code that was promised.
public class BicycleRepository : IBicycleRepository 
{
    private readonly AppDbContext _context;

    public BicycleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Bicycle>> GetAllAsync()
    {
        return await _context.Bicycles.AsNoTracking().ToListAsync();
    }

    public async Task<Bicycle?> GetByIdAsync(Guid id)
    {
       return await _context.Bicycles.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Bicycle>> GetByTypeAsync(BicycleType type)
    {
       return await _context.Bicycles.AsNoTracking()
            .Where(b => b.BicycleType == type)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bicycle>> GetAvailableAsync()
    {
       return await _context.Bicycles.AsNoTracking()
            .Where(b => b.IsAvailable)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bicycle>> SearchAsync(string brand, string? model)
    {
        var query = _context.Bicycles.AsNoTracking()
            .Where(b => b.Brand.ToLower().Contains(brand.ToLower()));

        if (!string.IsNullOrWhiteSpace(model))
            query = query.Where(b => b.Model.ToLower().Contains(model.ToLower()));

        return await query.ToListAsync();
    }

    public async Task AddAsync(Bicycle bicycle)
    {
        await _context.Bicycles.AddAsync(bicycle);
    }

    public Task UpdateAsync(Bicycle bicycle)
    {
        _context.Bicycles.Update(bicycle);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var bicycle = await _context.Bicycles.FindAsync(id);
        if (bicycle is not null)
            _context.Bicycles.Remove(bicycle);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
