using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RentalsController : ControllerBase
{
    private readonly IRentalPricingService _rentalPricingService;

    public RentalsController(IRentalPricingService rentalPricingService)
    {
        _rentalPricingService = rentalPricingService;
    }

    // POST /api/rentals/quote?bicycleId=...&hours=2
    [HttpPost("quote")]
    public async Task<ActionResult<RentalQuoteDto>> GetQuote(
        [FromQuery] Guid bicycleId,
        [FromQuery] int hours)
    {
        if (hours < 1) return BadRequest("Hours must be at least 1.");
        return Ok(await _rentalPricingService.CalculateQuoteAsync(bicycleId, hours));
    }

    // POST /api/rentals/quote/discount?bicycleId=...&hours=2&discountCode=STUDENT10
    [HttpPost("quote/discount")]
    public async Task<ActionResult<RentalQuoteDto>> GetQuoteWithDiscount(
        [FromQuery] Guid bicycleId,
        [FromQuery] int hours,
        [FromQuery] string discountCode)
    {
        if (hours < 1) return BadRequest("Hours must be at least 1.");
        return Ok(await _rentalPricingService.CalculateQuoteWithDiscountAsync(bicycleId, hours, discountCode));
    }

    // POST /api/rentals/start
    [HttpPost("start")]
    public async Task<ActionResult<RentalDto>> StartRental([FromBody] StartRentalDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");

        return Ok(await _rentalPricingService.StartRentalAsync(dto.BicycleId, userId));
    }

    // POST /api/rentals/{id}/complete
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<RentalDto>> CompleteRental(Guid id)
        => Ok(await _rentalPricingService.CompleteRentalAsync(id));

    // GET /api/rentals/active  — Admin only
    [HttpGet("active")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<RentalDto>>> GetActiveRentals()
        => Ok(await _rentalPricingService.ListActiveRentalsAsync());

    // GET /api/rentals/bicycle/{bicycleId}  — Admin only
    [HttpGet("bicycle/{bicycleId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<RentalDto>>> GetRentalHistory(Guid bicycleId)
        => Ok(await _rentalPricingService.ListRentalHistoryAsync(bicycleId));
}
