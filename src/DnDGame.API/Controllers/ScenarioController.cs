using DnDGame.BusinessLayer.Dtos.Scenarios;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

/// <summary>
/// Interactive scenario endpoints. The controller stays thin: it only resolves
/// DTOs from the application service. HTTP status codes come from the global
/// ExceptionHandlingMiddleware, which maps the DomainException codes thrown by
/// IScenarioService (404 for missing runs/scenes/locations, 409 for state
/// conflicts, 400 for malformed requests).
/// </summary>
[ApiController]
[Route("api/scenario")]
public class ScenarioController : ControllerBase
{
    private readonly IScenarioService _scenarioService;

    public ScenarioController(IScenarioService scenarioService)
    {
        _scenarioService = scenarioService;
    }

    [HttpGet("current/{playerId:int}")]
    public async Task<ActionResult<StorySceneDto>> GetCurrent(int playerId)
    {
        var result = await _scenarioService.GetCurrentAsync(playerId);
        return Ok(result);
    }

    [HttpPost("start/{playerId:int}")]
    public async Task<ActionResult<ScenarioProgressDto>> Start(int playerId)
    {
        var result = await _scenarioService.StartAsync(playerId);
        return CreatedAtAction(nameof(GetCurrent), new { playerId }, result);
    }

    [HttpPost("choice")]
    public async Task<ActionResult<ScenarioProgressDto>> SelectChoice([FromBody] SelectChoiceRequest request)
    {
        var result = await _scenarioService.SelectChoiceAsync(request);
        return Ok(result);
    }

    [HttpGet("locations")]
    public async Task<ActionResult<IReadOnlyList<LocationDto>>> GetLocations()
    {
        var result = await _scenarioService.GetLocationsAsync();
        return Ok(result);
    }

    [HttpGet("locations/{locationId:int}")]
    public async Task<ActionResult<LocationDto>> GetLocation(int locationId)
    {
        var result = await _scenarioService.GetLocationAsync(locationId);
        return Ok(result);
    }
}