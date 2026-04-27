using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

// This interface lives in Domain — it defines WHAT we need, not HOW it's done.
// The HOW lives in Infrastructure (BicycleRepository).
// This is the key to Dependency Inversion: high-level code depends on abstractions, not concrete classes.
public interface IBicycleRepository
{
    Task<IEnumerable<Bicycle>> GetAllAsync();
    Task<Bicycle?> GetByIdAsync(Guid id);
    Task<IEnumerable<Bicycle>> GetByTypeAsync(BicycleType type);
    Task<IEnumerable<Bicycle>> GetAvailableAsync();
    Task<IEnumerable<Bicycle>> SearchAsync(string brand, string? model);
    Task AddAsync(Bicycle bicycle);
    Task UpdateAsync(Bicycle bicycle);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
