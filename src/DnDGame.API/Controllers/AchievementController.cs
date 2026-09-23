using DnDGame.BusinessLayer.Dtos.Achievements;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

/// <summary>
/// Achievements endpoints. Read-only: the catalog and the current player's
/// progress. Progress itself is only ever advanced by the game-event hooks in
/// the application services (creation/quest/victory/unlock), never by an HTTP
/// GET — so loading this page grants nothing.
/// </summary>
[ApiController]
[Route("api/achievements")]
public class AchievementController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    /// <summary>The whole catalog, independent of any player.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AchievementDto>>> GetCatalog()
    {
        var result = await _achievementService.GetCatalogAsync();
        return Ok(result);
    }

    /// <summary>
    /// The current player's progress. 404 (via the global middleware's NOT_FOUND
    /// mapping) when the current player has no character yet — consistent with
    /// GET /api/character/current.
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<PlayerAchievementsDto>> GetCurrent()
    {
        var result = await _achievementService.GetProgressForCurrentPlayerAsync();
        return Ok(result);
    }

    /// <summary>
    /// The achievements screen payload, loadable after resuming the game: the whole
    /// catalog with the current player's progress, grouped into locked / in-progress /
    /// unlocked buckets. Same server-side player resolution (ICurrentPlayerService)
    /// and same 404 rule as GET /api/character/current — the client never sends a
    /// player id, so this works again on every page reload.
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<AchievementsOverviewDto>> GetOverview()
    {
        var result = await _achievementService.GetOverviewForCurrentPlayerAsync();
        return Ok(result);
    }
}