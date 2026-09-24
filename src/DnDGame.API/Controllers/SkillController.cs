using DnDGame.BusinessLayer.Dtos.Skills;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

/// <summary>
/// Skill-tree endpoints. The catalog is scoped to the current player's
/// character (Race, Class) and merged with their unlock state; unlocking is the
/// only write in this feature (see ISkillService remarks on why there's no
/// level/prerequisite gating yet).
/// </summary>
[ApiController]
[Route("api/skills")]
public class SkillController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    /// <summary>
    /// The current player's skill tree: their (Race, Class) catalog merged with
    /// unlock state and their spendable SkillPoints balance. 404 (via the global
    /// middleware's NOT_FOUND mapping) when the current player has no character
    /// yet — consistent with GET /api/character/current.
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<CharacterSkillsDto>> GetCurrent()
    {
        var result = await _skillService.GetSkillTreeForCurrentPlayerAsync();
        return Ok(result);
    }

    /// <summary>
    /// Unlocks one skill for the current player, spending SkillPoints, and
    /// returns their updated skill tree.
    /// </summary>
    [HttpPost("{id:int}/unlock")]
    public async Task<ActionResult<CharacterSkillsDto>> Unlock(int id)
    {
        var result = await _skillService.UnlockSkillForCurrentPlayerAsync(id);
        return Ok(result);
    }
}
