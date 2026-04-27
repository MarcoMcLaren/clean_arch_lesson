using Application.DTOs;

namespace Application.Interfaces;

public interface IRentalPricingService
{
    Task<RentalQuoteDto> CalculateQuoteAsync(Guid bicycleId, int hours);
    Task<RentalQuoteDto> CalculateQuoteWithDiscountAsync(Guid bicycleId, int hours, string discountCode);
    Task<RentalDto> StartRentalAsync(Guid bicycleId, string userId);
    Task<RentalDto> CompleteRentalAsync(Guid rentalId);
    Task<IEnumerable<RentalDto>> ListActiveRentalsAsync();
    Task<IEnumerable<RentalDto>> ListRentalHistoryAsync(Guid bicycleId);
}
