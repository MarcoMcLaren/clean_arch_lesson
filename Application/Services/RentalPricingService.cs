using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

// RentalPricingService demonstrates rich business logic living in the Application layer.
// Notice: no EF Core, no HTTP, no Identity — pure business rules.
// Multiple private methods decompose the pricing logic into readable, testable steps.
public class RentalPricingService : IRentalPricingService
{
    private readonly IBicycleRepository _bicycleRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IMapper _mapper;

    // Type-based price multipliers — each bicycle type has a different pricing tier
    private static readonly Dictionary<BicycleType, decimal> TypeMultipliers = new()
    {
        { BicycleType.Electric, 1.5m },
        { BicycleType.Mountain, 1.3m },
        { BicycleType.Road,     1.1m },
        { BicycleType.Hybrid,   1.0m },
        { BicycleType.BMX,      0.9m }
    };

    // Valid discount codes mapped to their discount percentage
    private static readonly Dictionary<string, decimal> DiscountCodes = new()
    {
        { "STUDENT10",  0.10m },
        { "WEEKEND15",  0.15m },
        { "BULK20",     0.20m }
    };

    private const int BulkHoursThreshold = 8;
    private const decimal BulkDiscountRate = 0.20m;

    public RentalPricingService(
        IBicycleRepository bicycleRepository,
        IRentalRepository rentalRepository,
        IMapper mapper)
    {
        _bicycleRepository = bicycleRepository;
        _rentalRepository = rentalRepository;
        _mapper = mapper;
    }

    // CalculateQuoteAsync orchestrates the pricing chain:
    // base price → type multiplier → bulk discount → rounding
    public async Task<RentalQuoteDto> CalculateQuoteAsync(Guid bicycleId, int hours)
    {
        var bicycle = await _bicycleRepository.GetByIdAsync(bicycleId)
            ?? throw new BicycleNotFoundException(bicycleId);

        var basePrice = CalculateBasePrice(bicycle.PricePerHour, hours);
        var multiplier = GetTypeMultiplier(bicycle.BicycleType);
        var priceAfterMultiplier = ApplyTypeMultiplier(basePrice, multiplier);
        var bulkDiscount = CalculateBulkDiscount(priceAfterMultiplier, hours);
        var finalPrice = RoundToTwoDecimals(priceAfterMultiplier - bulkDiscount);

        return new RentalQuoteDto
        {
            BicycleId = bicycle.Id,
            BicycleName = bicycle.Name,
            Hours = hours,
            BasePrice = basePrice,
            TypeMultiplier = multiplier,
            PriceAfterTypeMultiplier = priceAfterMultiplier,
            DiscountApplied = bulkDiscount,
            FinalPrice = finalPrice
        };
    }

    // CalculateQuoteWithDiscountAsync reuses the base quote and then stacks a discount code on top
    public async Task<RentalQuoteDto> CalculateQuoteWithDiscountAsync(Guid bicycleId, int hours, string discountCode)
    {
        var quote = await CalculateQuoteAsync(bicycleId, hours);

        if (!DiscountCodes.TryGetValue(discountCode.ToUpperInvariant(), out var rate))
            throw new InvalidRentalOperationException($"Discount code '{discountCode}' is not valid.");

        if (discountCode.ToUpperInvariant() == "BULK20" && hours < BulkHoursThreshold)
            throw new InvalidRentalOperationException("BULK20 discount requires at least 8 hours.");

        var discountAmount = CalculateDiscountAmount(quote.FinalPrice, rate);
        var finalPrice = RoundToTwoDecimals(quote.FinalPrice - discountAmount);

        quote.DiscountCode = discountCode.ToUpperInvariant();
        quote.DiscountApplied = RoundToTwoDecimals(quote.DiscountApplied + discountAmount);
        quote.FinalPrice = finalPrice;

        return quote;
    }

    public async Task<RentalDto> StartRentalAsync(Guid bicycleId, string userId)
    {
        var bicycle = await _bicycleRepository.GetByIdAsync(bicycleId)
            ?? throw new BicycleNotFoundException(bicycleId);

        if (!bicycle.IsAvailable)
            throw new BicycleNotAvailableException(bicycleId);

        // Mark bicycle unavailable so nobody else can rent it simultaneously
        bicycle.IsAvailable = false;
        await _bicycleRepository.UpdateAsync(bicycle);

        var rental = new Rental
        {
            Id = Guid.NewGuid(),
            BicycleId = bicycleId,
            UserId = userId,
            StartTime = DateTime.UtcNow,
            Status = RentalStatus.Active,
            Bicycle = bicycle
        };

        await _rentalRepository.AddAsync(rental);
        await _rentalRepository.SaveChangesAsync();
        await _bicycleRepository.SaveChangesAsync();

        return _mapper.Map<RentalDto>(rental);
    }

    public async Task<RentalDto> CompleteRentalAsync(Guid rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidRentalOperationException($"Rental '{rentalId}' not found.");

        if (rental.Status != RentalStatus.Active)
            throw new InvalidRentalOperationException("Only active rentals can be completed.");

        rental.EndTime = DateTime.UtcNow;
        rental.Status = RentalStatus.Completed;

        // Calculate actual cost based on real duration (minimum 1 hour)
        var hours = (int)Math.Ceiling((rental.EndTime.Value - rental.StartTime).TotalHours);
        var quote = await CalculateQuoteAsync(rental.BicycleId, Math.Max(hours, 1));
        rental.TotalCost = quote.FinalPrice;

        // Make the bicycle available again
        var bicycle = await _bicycleRepository.GetByIdAsync(rental.BicycleId)
            ?? throw new BicycleNotFoundException(rental.BicycleId);
        bicycle.IsAvailable = true;

        await _rentalRepository.UpdateAsync(rental);
        await _bicycleRepository.UpdateAsync(bicycle);
        await _rentalRepository.SaveChangesAsync();
        await _bicycleRepository.SaveChangesAsync();

        return _mapper.Map<RentalDto>(rental);
    }

    public async Task<IEnumerable<RentalDto>> ListActiveRentalsAsync()
    {
        var rentals = await _rentalRepository.GetActiveAsync();
        return _mapper.Map<IEnumerable<RentalDto>>(rentals);
    }

    public async Task<IEnumerable<RentalDto>> ListRentalHistoryAsync(Guid bicycleId)
    {
        var rentals = await _rentalRepository.GetByBicycleIdAsync(bicycleId);
        return _mapper.Map<IEnumerable<RentalDto>>(rentals);
    }

    // ── Private helper methods ─────────────────────────────────────────────────
    // Breaking logic into small methods makes each step readable and independently testable.

    private static decimal CalculateBasePrice(decimal pricePerHour, int hours)
        => pricePerHour * hours;

    private static decimal GetTypeMultiplier(BicycleType type)
        => TypeMultipliers.TryGetValue(type, out var multiplier) ? multiplier : 1.0m;

    private static decimal ApplyTypeMultiplier(decimal basePrice, decimal multiplier)
        => basePrice * multiplier;

    private static decimal CalculateBulkDiscount(decimal price, int hours)
        => hours >= BulkHoursThreshold ? RoundToTwoDecimals(price * BulkDiscountRate) : 0m;

    private static decimal CalculateDiscountAmount(decimal price, decimal rate)
        => RoundToTwoDecimals(price * rate);

    private static decimal RoundToTwoDecimals(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
