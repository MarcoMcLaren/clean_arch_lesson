using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

// AutoMapper profile: tells AutoMapper how to convert between domain entities and DTOs.
// DTOs are what we expose over the API — entities are our internal domain model.
// This keeps our API response shape decoupled from our database schema.
public class BicycleMappingProfile : Profile
{
    public BicycleMappingProfile()
    {
        CreateMap<Bicycle, BicycleDto>();

        CreateMap<CreateBicycleDto, Bicycle>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<UpdateBicycleDto, Bicycle>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<Rental, RentalDto>()
            .ForMember(dest => dest.BicycleName,
                opt => opt.MapFrom(src => src.Bicycle != null ? src.Bicycle.Name : string.Empty));
    }
}
