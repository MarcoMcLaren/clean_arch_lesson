using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

// BicycleService sits in the Application layer.
// It depends on IBicycleRepository (defined in Domain) — not on EF Core or SQL.
// This means we can swap the database engine without touching this class.
// Dependency Injection will supply the real repository at runtime.
public class BicycleService : IBicycleService
{
    
    //BicycleService depends on the IBicycleRepository and IMapper(AutoMapper) to fulfill the IBicycleService contract
    private readonly IBicycleRepository _repository; // this is a private variable we declare of type (not string, int, bool ) but of type IBicycleRepository we created
    private readonly IMapper _mapper;

    // This is called a constructor, here we will assign values to the private variables we just declared
    //BicycleService says in its constructor "I need an IBicycleRepository" and "I need a IMapper" to function, The DI container sees that, checks its registrations in Program.cs, and goes: "Someone is asking for IBicycleRepository — I know that one, give them a BicycleRepository."
    public BicycleService(IBicycleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BicycleDto>> ListBicyclesAsync()
    {
        var bicycles = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<BicycleDto>>(bicycles);
    }

    public async Task<BicycleDto> FindBicycleAsync(Guid id)
    {
        // If not found, throw a Domain exception — not an HTTP exception.
        // The middleware in Presentation will convert this to a 404 response.
        var bicycle = await _repository.GetByIdAsync(id)
            ?? throw new BicycleNotFoundException(id);

        return _mapper.Map<BicycleDto>(bicycle);
    }

    public async Task<IEnumerable<BicycleDto>> ListByTypeAsync(BicycleType type)
    {
        var bicycles = await _repository.GetByTypeAsync(type);
        return _mapper.Map<IEnumerable<BicycleDto>>(bicycles);
    }

    public async Task<IEnumerable<BicycleDto>> ListAvailableBicyclesAsync()
    {
        var bicycles = await _repository.GetAvailableAsync();
        return _mapper.Map<IEnumerable<BicycleDto>>(bicycles);
    }

    public async Task<IEnumerable<BicycleDto>> SearchBicyclesAsync(string brand, string? model)
    {
        var bicycles = await _repository.SearchAsync(brand, model);
        return _mapper.Map<IEnumerable<BicycleDto>>(bicycles);
    }

    public async Task<BicycleDto> RegisterBicycleAsync(CreateBicycleDto dto)
    {
        var bicycle = _mapper.Map<Bicycle>(dto);
        await _repository.AddAsync(bicycle);
        await _repository.SaveChangesAsync();
        return _mapper.Map<BicycleDto>(bicycle);
    }

    public async Task<BicycleDto> UpdateBicycleAsync(Guid id, UpdateBicycleDto dto)
    {
        var bicycle = await _repository.GetByIdAsync(id)
            ?? throw new BicycleNotFoundException(id);

        _mapper.Map(dto, bicycle);
        await _repository.UpdateAsync(bicycle);
        await _repository.SaveChangesAsync();
        return _mapper.Map<BicycleDto>(bicycle);
    }

    public async Task RemoveBicycleAsync(Guid id)
    {
        var bicycle = await _repository.GetByIdAsync(id)
            ?? throw new BicycleNotFoundException(id);

        await _repository.DeleteAsync(bicycle.Id);
        await _repository.SaveChangesAsync();
    }
}
