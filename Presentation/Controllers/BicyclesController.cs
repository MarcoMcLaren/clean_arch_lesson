using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

// TEACHING NOTE — Dependency Direction:
// This controller only imports from Application layer (IBicycleService, DTOs).
// It has ZERO knowledge of EF Core, SQL, repositories, or Identity.
// That's the Clean Architecture promise: Presentation → Application only.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BicyclesController : ControllerBase
{
    private readonly IBicycleService _bicycleService;

    public BicyclesController(IBicycleService bicycleService)
    {
        _bicycleService = bicycleService;
    }

    // GET /api/bicycles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BicycleDto>>> GetAll()
    {
        return Ok(await _bicycleService.ListBicyclesAsync());
    }

    // GET /api/bicycles/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BicycleDto>> GetById(Guid id)
    {
        return Ok(await _bicycleService.FindBicycleAsync(id));
    }

    // GET /api/bicycles/available
    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<BicycleDto>>> GetAvailable()
    {
        return Ok(await _bicycleService.ListAvailableBicyclesAsync());
    }

    // GET /api/bicycles/type/{type}
    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<BicycleDto>>> GetByType(BicycleType type)
    {
        return Ok(await _bicycleService.ListByTypeAsync(type));
    }

    // GET /api/bicycles/search?brand=Trek&model=X1
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<BicycleDto>>> Search(
        [FromQuery] string brand,
        [FromQuery] string? model = null)
    {
        if (string.IsNullOrWhiteSpace(brand))
            return BadRequest("Brand is required for search.");

        return Ok(await _bicycleService.SearchBicyclesAsync(brand, model));
    }

    // POST /api/bicycles  — Admin only
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<BicycleDto>> Create([FromBody] CreateBicycleDto dto)
    {
        var bicycle = await _bicycleService.RegisterBicycleAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = bicycle.Id }, bicycle);
    }

    // PUT /api/bicycles/{id}  — Admin only
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<BicycleDto>> Update(Guid id, [FromBody] UpdateBicycleDto dto)
    {
        return Ok(await _bicycleService.UpdateBicycleAsync(id, dto));
    }

    // DELETE /api/bicycles/{id}  — Admin only
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _bicycleService.RemoveBicycleAsync(id);
        return NoContent();
    }
}
