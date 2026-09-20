using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

/// <summary>
/// Dedicated read-only location catalog API for the location progression frontend.
/// </summary>
[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationProgressionService _locationProgressionService;

    public LocationsController(ILocationProgressionService locationProgressionService)
    {
        _locationProgressionService = locationProgressionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LocationSummaryDto>>> GetAll()
    {
        var result = await _locationProgressionService.GetAllLocationsAsync();
        return Ok(result);
    }

    [HttpGet("{locationId:int}")]
    public async Task<ActionResult<LocationDetailsDto>> GetDetails(int locationId)
    {
        var result = await _locationProgressionService.GetLocationDetailsAsync(locationId);
        return Ok(result);
    }

    [HttpPost("travel")]
    public async Task<ActionResult<TravelToLocationResultDto>> Travel([FromBody] TravelToLocationRequestDto request)
    {
        var result = await _locationProgressionService.TravelToLocationAsync(request);
        return Ok(result);
    }

    [HttpGet("player/{playerId:int}/route")]
    public async Task<ActionResult<IReadOnlyList<LocationRouteStepDto>>> GetPlayerRoute(int playerId)
    {
        var result = await _locationProgressionService.GetPlayerRouteAsync(playerId);
        return Ok(result);
    }

    [HttpGet("player/{playerId:int}")]
    public async Task<ActionResult<IReadOnlyList<LocationStatusDto>>> GetPlayerLocations(int playerId)
    {
        var result = await _locationProgressionService.GetPlayerLocationStatusesAsync(playerId);
        return Ok(result);
    }
}
