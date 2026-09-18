using DnDGame.BusinessLayer.Dtos.Progression;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

/// <summary>
/// Character progression endpoints. Thin controller delegating to
/// IProgressionService; HTTP status codes come from the global middleware
/// mapping the DomainExceptions the service throws (404 for unknown players,
/// 400 for non-positive experience amounts).
/// </summary>
[ApiController]
[Route("api/progression")]
public class ProgressionController : ControllerBase
{
    private readonly IProgressionService _progressionService;

    public ProgressionController(IProgressionService progressionService)
    {
        _progressionService = progressionService;
    }

    [HttpGet("{playerId:int}")]
    public async Task<ActionResult<CharacterProgressionDto>> Get(int playerId)
    {
        var result = await _progressionService.GetProgressionAsync(playerId);
        return Ok(result);
    }

    [HttpPost("{playerId:int}/experience")]
    public async Task<ActionResult<CharacterProgressionDto>> GrantExperience(
        int playerId,
        [FromBody] ExperienceGainRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await _progressionService.GrantExperienceAsync(playerId, request.Amount);
        return Ok(result);
    }
}