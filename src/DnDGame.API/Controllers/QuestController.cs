using DnDGame.BusinessLayer.Models;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

/// <summary>
/// Quest endpoints, keyed by player id. Thin controller: every endpoint delegates
/// to IQuestService, and HTTP status codes come from the global middleware's
/// mapping of the DomainExceptions the service throws (404 for missing players,
/// characters, sessions or quests; 409 for quest-state conflicts; 400 for bad input).
/// </summary>
[ApiController]
[Route("api/quests")]
public class QuestController : ControllerBase
{
    private readonly IQuestService _questService;

    public QuestController(IQuestService questService)
    {
        _questService = questService;
    }

    [HttpGet("{playerId:int}/available")]
    public async Task<ActionResult<IReadOnlyList<QuestView>>> GetAvailable(int playerId, [FromQuery] int? locationId = null)
    {
        var result = await _questService.GetAvailableQuestsForPlayerAsync(playerId, locationId);
        return Ok(result);
    }

    [HttpGet("{playerId:int}/active")]
    public async Task<ActionResult<IReadOnlyList<QuestProgressView>>> GetActive(int playerId)
    {
        var result = await _questService.GetActiveQuestsForPlayerAsync(playerId);
        return Ok(result);
    }

    [HttpGet("{playerId:int}/completed")]
    public async Task<ActionResult<IReadOnlyList<QuestProgressView>>> GetCompleted(int playerId)
    {
        var result = await _questService.GetCompletedQuestsForPlayerAsync(playerId);
        return Ok(result);
    }

    [HttpPost("{playerId:int}/{questId:int}/start")]
    public async Task<ActionResult<QuestProgressView>> Start(int playerId, int questId)
    {
        var result = await _questService.StartQuestForPlayerAsync(playerId, questId);
        return Ok(result);
    }

    [HttpPost("{playerId:int}/{questId:int}/complete")]
    public async Task<ActionResult<QuestCompletionResult>> Complete(int playerId, int questId)
    {
        var result = await _questService.CompleteQuestForPlayerAsync(playerId, questId);
        return Ok(result);
    }
}