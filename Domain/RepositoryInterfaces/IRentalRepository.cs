using Domain.Entities;

namespace Domain.Interfaces;

public interface IRentalRepository
{
    Task<Rental?> GetByIdAsync(Guid id);
    Task<IEnumerable<Rental>> GetActiveAsync();
    Task<IEnumerable<Rental>> GetByBicycleIdAsync(Guid bicycleId);
    Task AddAsync(Rental rental);
    Task UpdateAsync(Rental rental);
    Task SaveChangesAsync();
}
