using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Locations;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

/// <summary>
/// Location-progression endpoints (BACK-LOC-08). Thin controller: every endpoint
/// delegates to ILocationService, and HTTP status codes come from the global
/// middleware's mapping of the DomainExceptions the service throws (404 for
/// missing players/characters/sessions/locations, 400 for bad input/unmet unlock
/// requirements, 409 for unlock-state conflicts).
/// </summary>
[ApiController]
[Route("api/locations")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LocationSummaryDto>>> GetAll()
    {
        var result = await _locationService.GetAllLocationsAsync();
        return Ok(result);
    }

    [HttpGet("player/{playerId:int}")]
    public async Task<ActionResult<LocationProgressDto>> GetProgressForPlayer(int playerId)
    {
        var result = await _locationService.GetProgressForPlayerAsync(playerId);
        return Ok(result);
    }

    [HttpGet("player/{playerId:int}/route")]
    public async Task<ActionResult<LocationRouteDto>> GetRouteForPlayer(int playerId)
    {
        var result = await _locationService.GetRouteForPlayerAsync(playerId);
        return Ok(result);
    }

    [HttpGet("{locationId}/enemies")]
    public async Task<ActionResult<IReadOnlyList<LocationEnemyDto>>> GetAvailableEnemies(
        LocationId locationId, [FromQuery] int playerId, [FromQuery] string? subLocation = null)
    {
        var result = await _locationService.GetAvailableEnemiesAsync(locationId, playerId, subLocation);
        return Ok(result);
    }

    [HttpGet("{locationId}")]
    public async Task<ActionResult<LocationDetailsDto>> GetById(LocationId locationId)
    {
        var result = await _locationService.GetLocationByIdAsync(locationId);
        return Ok(result);
    }

    [HttpPost("travel")]
    public async Task<ActionResult<TravelToLocationResultDto>> Travel([FromBody] TravelToLocationRequest request)
    {
        var result = await _locationService.TravelToLocationAsync(request);
        return Ok(result);
    }
}
