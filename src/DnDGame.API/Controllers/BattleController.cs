using DnDGame.BusinessLayer.Dtos.Battles;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

[ApiController]
[Route("api/battle")]
public class BattleController : ControllerBase
{
    private readonly IBattleService _battleService;

    public BattleController(IBattleService battleService)
    {
        _battleService = battleService;
    }

    [HttpPost("start")]
    public async Task<ActionResult<BattleStateDto>> Start([FromBody] StartBattleRequestDto request)
    {
        var result = await _battleService.StartBattleAsync(request);
        return CreatedAtAction(nameof(GetById), new { battleId = result.Id }, result);
    }

    [HttpGet("{battleId:int}")]
    public async Task<ActionResult<BattleStateDto>> GetById(int battleId)
    {
        var result = await _battleService.GetBattleStateAsync(battleId);
        return Ok(result);
    }

    [HttpPost("{battleId:int}/play-card")]
    public async Task<ActionResult<BattleStateDto>> PlayCard(int battleId, [FromBody] PlayCardRequestDto request)
    {
        var result = await _battleService.PlayCardAsync(battleId, request);
        return Ok(result);
    }

    [HttpPost("{battleId:int}/end-turn")]
    public async Task<ActionResult<BattleStateDto>> EndTurn(int battleId)
    {
        var result = await _battleService.EndTurnAsync(battleId);
        return Ok(result);
    }

    [HttpGet("{battleId:int}/log")]
    public async Task<ActionResult<IReadOnlyList<BattleLogEntryDto>>> GetLog(int battleId)
    {
        var result = await _battleService.GetBattleLogAsync(battleId);
        return Ok(result);
    }
}
