using DnDGame.BusinessLayer.Dtos.Achievements;
using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Read + event-side of the achievements feature.
///
/// The Register* methods are the ONLY writers: they are invoked from the
/// application services exactly where the corresponding real game event is
/// persisted (character creation, quest completion, battle victory, location
/// unlock). They are intentionally not part of any HTTP surface — no GET endpoint
/// can advance an achievement, so loading a page grants nothing.
///
/// Each Register* call names the identity of the specific game record behind the
/// event (quest id, battle id, location id). That identity is ledgered per player,
/// so re-delivering the same event (a re-evaluated victory, a quest finalized
/// through two paths, ...) is a no-op: the same event never grants an achievement
/// twice, and re-reporting it never inflates progress toward a threshold.
/// </summary>
public interface IAchievementService
{
    /// <summary>The whole catalog, independent of any player.</summary>
    Task<IReadOnlyList<AchievementDto>> GetCatalogAsync();

    /// <summary>
    /// The current player's progress. Resolves the player server-side via
    /// ICurrentPlayerService and keys the result on their character id. Throws
    /// DomainException(NOT_FOUND) if the current player has no character yet.
    /// </summary>
    Task<PlayerAchievementsDto> GetProgressForCurrentPlayerAsync();

    /// <summary>Progress keyed directly by a character id (used by tests and player-keyed endpoints).</summary>
    Task<PlayerAchievementsDto> GetProgressForPlayerAsync(int characterId);

    /// <summary>
    /// The current player's full catalog/progress view for the achievements screen,
    /// after the game has been resumed. Same server-side player resolution and the
    /// same 404 rule as <see cref="GetProgressForCurrentPlayerAsync"/>, but the
    /// entries are grouped into locked / in-progress / unlocked buckets so the
    /// frontend can render the three sections without client-side filtering.
    /// </summary>
    Task<AchievementsOverviewDto> GetOverviewForCurrentPlayerAsync();

    Task RegisterCharacterCreatedAsync(int characterId);
    Task RegisterQuestCompletedAsync(int characterId, int questId);
    Task RegisterBattleVictoryAsync(int characterId, int battleId);
    Task RegisterLocationUnlockedAsync(int characterId, LocationId locationId);
}