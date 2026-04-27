using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces;

public interface IBicycleService
{
    Task<IEnumerable<BicycleDto>> ListBicyclesAsync();
    Task<BicycleDto> FindBicycleAsync(Guid id);
    Task<IEnumerable<BicycleDto>> ListByTypeAsync(BicycleType type);
    Task<IEnumerable<BicycleDto>> ListAvailableBicyclesAsync();
    Task<IEnumerable<BicycleDto>> SearchBicyclesAsync(string brand, string? model);
    Task<BicycleDto> RegisterBicycleAsync(CreateBicycleDto dto);
    Task<BicycleDto> UpdateBicycleAsync(Guid id, UpdateBicycleDto dto);
    Task RemoveBicycleAsync(Guid id);
}
